using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Newtonsoft.Json;
using ECommBackend.Models;
using ECommBackend.Models.ModInterfaces;
using ECommBackend.Repositories.RepoInterfaces;


namespace ECommBackend.Services
{
    public class AzureBackgroundService:BackgroundService
    {
        private const string QueueName = "OrderPlaced";

        private readonly ILogger<AzureBackgroundService> _logger;

        
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusAdministrationClient _serviceBusAdministrationClient;

        public AzureBackgroundService(
            ILogger<AzureBackgroundService> logger,
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;

            var connectionString = configuration.GetSection("SecretKeys")["PrimaryConn"];
            _serviceBusClient = new ServiceBusClient(connectionString);
            _serviceBusAdministrationClient = new ServiceBusAdministrationClient(connectionString);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!await _serviceBusAdministrationClient.QueueExistsAsync(QueueName, stoppingToken))
            {
                _logger.LogError("Queue {QueueName} doesn't exist in the current scope", QueueName);
                return;
            }

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await DrainNextSessionAsync(stoppingToken);
                    }
                    catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.ServiceTimeout)
                    {
                       
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Session processing on {QueueName} failed", QueueName);
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Request Cancellation is requested");
            }
        }

      
        private async Task DrainNextSessionAsync(CancellationToken stoppingToken)
        {
            await using var receiver = await _serviceBusClient.AcceptNextSessionAsync(
                QueueName,
                cancellationToken: stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var message = await receiver.ReceiveMessageAsync(cancellationToken: stoppingToken);
                if (message is null)
                {
                    return;
                }

                await HandleOrderPlacedAsync(receiver, message, stoppingToken);
            }
        }

        private async Task HandleOrderPlacedAsync(
            ServiceBusSessionReceiver receiver,
            ServiceBusReceivedMessage message,
            CancellationToken ctx)
        {
            OrderModel? order;
            try
            {
                order = JsonConvert.DeserializeObject<OrderModel>(message.Body.ToString());
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Message {MessageId} isn't a readable order", message.MessageId);
                await receiver.DeadLetterMessageAsync(message, "UnreadablePayload", ex.Message, ctx);
                return;
            }

            if (order is null || order.Products.Count == 0)
            {
                _logger.LogError("Message {MessageId} carries no products", message.MessageId);
                await receiver.DeadLetterMessageAsync(message, "NoProducts", "Order carries no products", ctx);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var variantService = scope.ServiceProvider.GetRequiredService<VariantService>();
            var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();

            if (!TryBuildReservations(order, out var reservations, out var rejectionReason))
            {
                _logger.LogWarning(
                    "Order {OrderId} rejected: {Reason}",
                    order.OrderId,
                    rejectionReason);

                await orderService.UpdateOrderStatus(order.OrderId, OrderStatus.Rejected, ctx);
                await receiver.CompleteMessageAsync(message, ctx);
                return;
            }

       
            if (!await variantService.TryReserveUnits(reservations, ctx))
            {
                _logger.LogWarning("Order {OrderId} rejected: insufficient stock", order.OrderId);

                await orderService.UpdateOrderStatus(order.OrderId, OrderStatus.Rejected, ctx);
                await receiver.CompleteMessageAsync(message, ctx);
                return;
            }

            await orderService.UpdateOrderStatus(order.OrderId, OrderStatus.Processed, ctx);
            await receiver.CompleteMessageAsync(message, ctx);

            _logger.LogInformation(
                "Order {OrderId} processed, {LineCount} variant(s) reserved",
                order.OrderId,
                reservations.Count);
        }


        private static bool TryBuildReservations(
            OrderModel order,
            out IReadOnlyCollection<VariantReservation> reservations,
            out string? rejectionReason)
        {
            var built = new List<VariantReservation>(order.Products.Count);

            foreach (var product in order.Products)
            {
                var variant = product.Variants
                    .Where(x => x.Units > 0)
                    .OrderBy(x => x.Price)
                    .FirstOrDefault();

                if (variant is null)
                {
                    reservations = Array.Empty<VariantReservation>();
                    rejectionReason = $"product {product.ProductId} has no variant in stock";
                    return false;
                }

                built.Add(new VariantReservation(variant.VariantId, 1));
            }

            reservations = built;
            rejectionReason = null;
            return true;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await _serviceBusClient.DisposeAsync();
        }
    }
}

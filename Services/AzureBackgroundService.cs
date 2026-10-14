using System.Diagnostics;
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
        private const string QueueName = AzureServiceBusClass.OrderQueueName;

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
                _logger.LogWarning("Queue {QueueName} doesn't exist, creating it", QueueName);
                await _serviceBusAdministrationClient.CreateQueueAsync(
                    AzureServiceBusClass.OrderQueueOptions(), stoppingToken);
            }

            await using var receiver = _serviceBusClient.CreateReceiver(QueueName);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var message = await receiver.ReceiveMessageAsync(cancellationToken: stoppingToken);
                        if (message is null)
                        {
                            continue;
                        }

                        await HandleOrderPlacedAsync(receiver, message, stoppingToken);
                    }
                    catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.ServiceTimeout)
                    {
                       
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Message processing on {QueueName} failed", QueueName);
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Request Cancellation is requested");
            }
        }


        private async Task HandleOrderPlacedAsync(
            ServiceBusReceiver receiver,
            ServiceBusReceivedMessage message,
            CancellationToken ctx)
        {
            using var activity = MessageTracing.StartProcess(
                OrderTelemetry.Source,
                QueueName,
                message.MessageId,
                message.DeliveryCount,
                message.ApplicationProperties);

            var started = Stopwatch.GetTimestamp();
            var outcome = OrderTelemetry.OutcomeFailed;
            try
            {
                outcome = await ProcessOrderAsync(receiver, message, activity, ctx);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                activity.RecordException(ex);
                throw;
            }
            finally
            {
                activity?.SetTag("order.outcome", outcome);
                OrderTelemetry.RecordProcessed(outcome, Stopwatch.GetElapsedTime(started));
            }
        }

        // Returns the outcome so the span tag and the metrics are recorded in one place.
        private async Task<string> ProcessOrderAsync(
            ServiceBusReceiver receiver,
            ServiceBusReceivedMessage message,
            Activity? activity,
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
                return "dead_lettered";
            }

            if (order is null || order.Products.Count == 0)
            {
                _logger.LogError("Message {MessageId} carries no products", message.MessageId);
                await receiver.DeadLetterMessageAsync(message, "NoProducts", "Order carries no products", ctx);
                return "dead_lettered";
            }

            activity?.SetTag("order.id", order.OrderId);

            using var scope = _scopeFactory.CreateScope();
            var variantService = scope.ServiceProvider.GetRequiredService<VariantService>();
            var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();
            var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepo>();


            var storedOrder = await orderRepo.GetSingleOrder(order.OrderId, ctx);
            if (storedOrder.OrderStatus != OrderStatus.Pending)
            {
                _logger.LogInformation(
                    "Order {OrderId} already {Status}, skipping duplicate message {MessageId}",
                    order.OrderId,
                    storedOrder.OrderStatus,
                    message.MessageId);
                await receiver.CompleteMessageAsync(message, ctx);
                return "duplicate";
            }

            if (!TryBuildReservations(order, out var reservations, out var rejectionReason))
            {
                _logger.LogWarning(
                    "Order {OrderId} rejected: {Reason}",
                    order.OrderId,
                    rejectionReason);

                activity?.SetTag("order.rejection_reason", rejectionReason);
                await orderService.UpdateOrderStatus(order.OrderId, OrderStatus.Rejected, ctx);
                await receiver.CompleteMessageAsync(message, ctx);
                return "rejected";
            }

       
            if (!await variantService.TryReserveUnits(reservations, ctx))
            {
                _logger.LogWarning("Order {OrderId} rejected: insufficient stock", order.OrderId);
                activity?.SetTag("order.rejection_reason", "insufficient stock");

                await orderService.UpdateOrderStatus(order.OrderId, OrderStatus.Rejected, ctx);
                await receiver.CompleteMessageAsync(message, ctx);
                return "rejected";
            }

            await orderService.UpdateOrderStatus(order.OrderId, OrderStatus.Processed, ctx);
            await receiver.CompleteMessageAsync(message, ctx);

            _logger.LogInformation(
                "Order {OrderId} processed, {LineCount} variant(s) reserved",
                order.OrderId,
                reservations.Count);
            return "processed";
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

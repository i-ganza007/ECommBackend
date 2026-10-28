using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Newtonsoft.Json;
using ECommBackend.Models;
using ECommBackend.Models.ModInterfaces;
using ECommBackend.Repositories.RepoInterfaces;
using Microsoft.Extensions.Options;


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
            IOptions<ServiceBusSettings> serviceBusSettings)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;

            var connectionString = serviceBusSettings.Value.PrimaryConn;
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

            var outcome = "failed";
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
            }
        }

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
            var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();
            var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepo>();


            var storedOrder = await orderRepo.GetSingleOrder(order.OrderId, ctx);
            if (storedOrder.OrderStatus == OrderStatus.Cancelled)
            {
                return await SkipCancelledOrderAsync(receiver, message, order.OrderId, ctx);
            }

            if (storedOrder.OrderStatus != OrderStatus.Pending)
            {
                return await SkipDuplicateAsync(receiver, message, order.OrderId, storedOrder.OrderStatus, ctx);
            }

            // When some product has nothing in stock the reservations come back empty, and
            // TryFinalizeOrder rejects the order inside the same guarded transaction.
            var hasStockForEveryProduct = TryBuildReservations(order, out var reservations, out var rejectionDetail);

            var result = await orderRepo.TryFinalizeOrder(order.OrderId, reservations, ctx);
            switch (result)
            {
                case FinalizeResult.Cancelled:
                    return await SkipCancelledOrderAsync(receiver, message, order.OrderId, ctx);

                case FinalizeResult.AlreadyFinalized:
                    return await SkipDuplicateAsync(receiver, message, order.OrderId, null, ctx);

                case FinalizeResult.Rejected:
                    var rejectionReason = hasStockForEveryProduct ? "insufficient_stock" : "no_variant_in_stock";
                    _logger.LogWarning(
                        "Order {OrderId} rejected: {Reason} {Detail}",
                        order.OrderId,
                        rejectionReason,
                        rejectionDetail);

                    activity?.SetTag("order.rejection_reason", rejectionReason);
                    await orderService.OnOrderFinalized(order.OrderId, OrderTelemetry.OutcomeRejected, storedOrder.CreatedDate, ctx, rejectionReason);
                    await receiver.CompleteMessageAsync(message, ctx);
                    return "rejected";
            }

            await orderService.OnOrderFinalized(order.OrderId, OrderTelemetry.OutcomeConfirmed, storedOrder.CreatedDate, ctx);
            await receiver.CompleteMessageAsync(message, ctx);

            _logger.LogInformation(
                "Order {OrderId} processed, {LineCount} variant(s) reserved",
                order.OrderId,
                reservations.Count);
            return OrderTelemetry.OutcomeConfirmed;
        }


        private async Task<string> SkipCancelledOrderAsync(
            ServiceBusReceiver receiver,
            ServiceBusReceivedMessage message,
            Guid orderId,
            CancellationToken ctx)
        {
            _logger.LogInformation(
                "Order {OrderId} was cancelled, skipping message {MessageId}",
                orderId,
                message.MessageId);
            await receiver.CompleteMessageAsync(message, ctx);
            return "cancelled_skipped";
        }

        private async Task<string> SkipDuplicateAsync(
            ServiceBusReceiver receiver,
            ServiceBusReceivedMessage message,
            Guid orderId,
            OrderStatus? status,
            CancellationToken ctx)
        {
            _logger.LogInformation(
                "Order {OrderId} already {Status}, skipping duplicate message {MessageId}",
                orderId,
                status?.ToString() ?? "finalized",
                message.MessageId);
            await receiver.CompleteMessageAsync(message, ctx);
            return "duplicate";
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

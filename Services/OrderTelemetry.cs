using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ECommBackend.Services
{
    public static class OrderTelemetry
    {
        public const string Name = "ECommBackend.Orders";

        public static readonly ActivitySource Source = new(Name);
        private static readonly Meter Meter = new(Name);

        public const string OutcomeConfirmed = "confirmed";
        public const string OutcomeRejected = "rejected";

        private static readonly Counter<long> OrdersPlaced = Meter.CreateCounter<long>(
            "ecomm.orders.placed", "{order}", "Orders accepted by the API and queued for processing");

        private static readonly Histogram<double> OrderValue = Meter.CreateHistogram<double>(
            "ecomm.orders.value", description: "Total price of each placed order");

        private static readonly Counter<long> OrdersProcessed = Meter.CreateCounter<long>(
            "ecomm.orders.processed", "{order}", "Orders that reached a final status, by outcome");

        private static readonly Histogram<double> ProcessingDuration = Meter.CreateHistogram<double>(
            "ecomm.orders.processing.duration", "ms", "Time from Pending to saving a final order status");

        private static long _pendingOrders;
        private static readonly ObservableGauge<long> PendingOrders = Meter.CreateObservableGauge(
            "ecomm.orders.pending", () => Interlocked.Read(ref _pendingOrders), "{order}",
            "Number of orders currently in Pending status");

        private static readonly Counter<long> MessagesPublished = Meter.CreateCounter<long>(
            "ecomm.messages.published", "{message}", "Messages sent to Service Bus, by queue");

        public static void RecordPlaced(double totalPrice)
        {
            OrdersPlaced.Add(1);
            OrderValue.Record(totalPrice);
        }

        public static void RecordFinalized(string outcome, string? rejectionReason, double elapsedMilliseconds)
        {
            var tags = new TagList { { "order.outcome", outcome } };
            if (outcome == OutcomeRejected && rejectionReason is not null)
            {
                tags.Add("order.rejection_reason", rejectionReason);
            }

            OrdersProcessed.Add(1, tags);
            ProcessingDuration.Record(elapsedMilliseconds, tags);
        }

        public static void SetPendingOrders(long count) => Interlocked.Exchange(ref _pendingOrders, count);

        public static void RecordPublished(string queueName) =>
            MessagesPublished.Add(1, new KeyValuePair<string, object?>("messaging.destination.name", queueName));

        public static void RecordException(this Activity? activity, Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
        }
    }
}

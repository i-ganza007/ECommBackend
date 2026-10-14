using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ECommBackend.Services
{
    // Spans and metrics for the order pipeline. Both names match the ECommBackend.* filters in ServiceDefaults.
    public static class OrderTelemetry
    {
        public const string Name = "ECommBackend.Orders";

        public static readonly ActivitySource Source = new(Name);
        private static readonly Meter Meter = new(Name);

        // Used when processing throws before reaching an outcome.
        public const string OutcomeFailed = "failed";

        private static readonly Counter<long> OrdersPlaced = Meter.CreateCounter<long>(
            "ecomm.orders.placed", "{order}", "Orders accepted by the API and queued for processing");

        private static readonly Histogram<double> OrderValue = Meter.CreateHistogram<double>(
            "ecomm.orders.value", description: "Total price of each placed order");

        private static readonly Counter<long> OrdersProcessed = Meter.CreateCounter<long>(
            "ecomm.orders.processed", "{message}", "Order messages handled, by outcome");

        private static readonly Histogram<double> ProcessingDuration = Meter.CreateHistogram<double>(
            "ecomm.orders.processing.duration", "s", "Time to handle one order message, stock reservation included");

        private static readonly Counter<long> MessagesPublished = Meter.CreateCounter<long>(
            "ecomm.messages.published", "{message}", "Messages sent to Service Bus, by queue");

        public static void RecordPlaced(double totalPrice, int productCount)
        {
            var tags = new TagList { { "order.product_count", productCount } };
            OrdersPlaced.Add(1, tags);
            OrderValue.Record(totalPrice, tags);
        }

        public static void RecordProcessed(string outcome, TimeSpan elapsed)
        {
            var tags = new TagList { { "order.outcome", outcome } };
            OrdersProcessed.Add(1, tags);
            ProcessingDuration.Record(elapsed.TotalSeconds, tags);
        }

        public static void RecordPublished(string queueName) =>
            MessagesPublished.Add(1, new KeyValuePair<string, object?>("messaging.destination.name", queueName));

        public static void RecordException(this Activity? activity, Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
        }
    }
}

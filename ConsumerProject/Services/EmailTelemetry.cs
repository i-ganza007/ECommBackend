using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ConsumerProject.Services;

// Email metrics. The name matches the ECommBackend.* meter filter in ServiceDefaults.
public static class EmailTelemetry
{
    public const string Name = "ECommBackend.EmailConsumer";

    private static readonly Meter Meter = new(Name);

    private static readonly Counter<long> EmailsProcessed = Meter.CreateCounter<long>(
        "ecomm.emails.processed", "{message}", "Email requests handled, by outcome (sent, retry, dead_lettered)");

    private static readonly Histogram<double> SendDuration = Meter.CreateHistogram<double>(
        "ecomm.emails.send.duration", "s", "Time for one SMTP round trip: connect, authenticate, send, disconnect");

    public static void RecordProcessed(string outcome, string? orderStatus = null)
    {
        var tags = new TagList { { "email.outcome", outcome } };
        if (orderStatus is not null)
        {
            tags.Add("order.status", orderStatus);
        }
        EmailsProcessed.Add(1, tags);
    }

    public static void RecordSendDuration(TimeSpan elapsed, bool succeeded) =>
        SendDuration.Record(elapsed.TotalSeconds, new KeyValuePair<string, object?>("email.succeeded", succeeded));
}

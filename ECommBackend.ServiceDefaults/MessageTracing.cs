using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Microsoft.Extensions.Hosting;


public static class MessageTracing
{
    private static TextMapPropagator Propagator => Propagators.DefaultTextMapPropagator;

    public static void Inject(Activity? activity, IDictionary<string, object> properties)
    {
        var context = activity?.Context ?? Activity.Current?.Context;
        if (context is null)
        {
            return;
        }

        Propagator.Inject(
            new PropagationContext(context.Value, Baggage.Current),
            properties,
            static (props, key, value) => props[key] = value);
    }

    public static ActivityContext Extract(IReadOnlyDictionary<string, object> properties)
    {
        var parent = Propagator.Extract(
            default,
            properties,
            static (props, key) => props.TryGetValue(key, out var value) && value?.ToString() is { } text
                ? [text]
                : []);

        Baggage.Current = parent.Baggage;
        return parent.ActivityContext;
    }

    // Tags follow the OpenTelemetry messaging conventions so the dashboard groups them properly.
    public static Activity? StartPublish(ActivitySource source, string queueName, string messageId)
    {
        var activity = source.StartActivity($"publish {queueName}", ActivityKind.Producer);
        activity?.SetTag("messaging.system", "servicebus");
        activity?.SetTag("messaging.operation.type", "send");
        activity?.SetTag("messaging.destination.name", queueName);
        activity?.SetTag("messaging.message.id", messageId);
        return activity;
    }

    public static Activity? StartProcess(
        ActivitySource source,
        string queueName,
        string messageId,
        int deliveryCount,
        IReadOnlyDictionary<string, object> properties)
    {
        var activity = source.StartActivity($"process {queueName}", ActivityKind.Consumer, Extract(properties));
        activity?.SetTag("messaging.system", "servicebus");
        activity?.SetTag("messaging.operation.type", "process");
        activity?.SetTag("messaging.destination.name", queueName);
        activity?.SetTag("messaging.message.id", messageId);
        activity?.SetTag("messaging.servicebus.message.delivery_count", deliveryCount);
        return activity;
    }
}

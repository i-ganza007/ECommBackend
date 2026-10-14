using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using ConsumerProject.Models;
using Newtonsoft.Json;

namespace ConsumerProject.Services;

public class ConsumerBackground : BackgroundService
{
    public const string EmailQueueName = "orderemailqueue";

    // Matches the ECommBackend.* source ServiceDefaults listens to.
    public static readonly ActivitySource Source = new(EmailTelemetry.Name);

    private readonly ILogger<ConsumerBackground> _logger;
    private readonly EmailSender _emailSender;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusAdministrationClient _serviceBusAdministrationClient;

    public ConsumerBackground(ILogger<ConsumerBackground> logger, EmailSender emailSender, IConfiguration configuration)
    {
        _logger = logger;
        _emailSender = emailSender;

        var connectionString = configuration["SecretKeys:PrimaryConn"]
            ?? throw new InvalidOperationException("SecretKeys:PrimaryConn isn't configured");
        _serviceBusClient = new ServiceBusClient(connectionString);
        _serviceBusAdministrationClient = new ServiceBusAdministrationClient(connectionString);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await _serviceBusAdministrationClient.QueueExistsAsync(EmailQueueName, stoppingToken))
        {
            _logger.LogWarning("Queue {QueueName} doesn't exist, creating it", EmailQueueName);
            await _serviceBusAdministrationClient.CreateQueueAsync(new CreateQueueOptions(EmailQueueName)
            {
                MaxDeliveryCount = 5,
                LockDuration = TimeSpan.FromMinutes(1)
            }, stoppingToken);
        }

        await using var receiver = _serviceBusClient.CreateReceiver(EmailQueueName);

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

                    await HandleEmailMessageAsync(receiver, message, stoppingToken);
                }
                catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.ServiceTimeout)
                {
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Message processing on {QueueName} failed", EmailQueueName);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Request Cancellation is requested");
        }
    }

    private async Task HandleEmailMessageAsync(
        ServiceBusReceiver receiver,
        ServiceBusReceivedMessage message,
        CancellationToken ctx)
    {
        // Continues the trace from the producer, so the email shows up under the order that caused it.
        using var activity = MessageTracing.StartProcess(
            Source,
            EmailQueueName,
            message.MessageId,
            message.DeliveryCount,
            message.ApplicationProperties);

        OrderEmailMessage? email;
        try
        {
            email = JsonConvert.DeserializeObject<OrderEmailMessage>(message.Body.ToString());
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Message {MessageId} isn't a readable email request", message.MessageId);
            activity?.SetStatus(ActivityStatusCode.Error, "Unreadable payload");
            activity?.SetTag("email.outcome", "dead_lettered");
            EmailTelemetry.RecordProcessed("dead_lettered");
            await receiver.DeadLetterMessageAsync(message, "UnreadablePayload", ex.Message, ctx);
            return;
        }

        if (email is null || string.IsNullOrWhiteSpace(email.ToEmail))
        {
            _logger.LogError("Message {MessageId} has no recipient", message.MessageId);
            activity?.SetStatus(ActivityStatusCode.Error, "No recipient");
            activity?.SetTag("email.outcome", "dead_lettered");
            EmailTelemetry.RecordProcessed("dead_lettered");
            await receiver.DeadLetterMessageAsync(message, "NoRecipient", "Email request has no recipient", ctx);
            return;
        }

        activity?.SetTag("order.id", email.OrderId);
        activity?.SetTag("order.status", email.OrderStatus.ToString());

        var started = Stopwatch.GetTimestamp();
        try
        {
            await _emailSender.SendOrderEmail(email, ctx);
            EmailTelemetry.RecordSendDuration(Stopwatch.GetElapsedTime(started), succeeded: true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            
            _logger.LogWarning(ex,
                "Sending email for order {OrderId} failed (attempt {Attempt})",
                email.OrderId,
                message.DeliveryCount);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            activity?.SetTag("email.outcome", "retry");
            EmailTelemetry.RecordSendDuration(Stopwatch.GetElapsedTime(started), succeeded: false);
            EmailTelemetry.RecordProcessed("retry", email.OrderStatus.ToString());
            await receiver.AbandonMessageAsync(message, cancellationToken: ctx);
            return;
        }

        activity?.SetTag("email.outcome", "sent");
        EmailTelemetry.RecordProcessed("sent", email.OrderStatus.ToString());
        await receiver.CompleteMessageAsync(message, ctx);
        _logger.LogInformation("Sent {Status} email for order {OrderId}", email.OrderStatus, email.OrderId);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await _serviceBusClient.DisposeAsync();
    }
}

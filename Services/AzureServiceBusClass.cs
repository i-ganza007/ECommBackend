using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus.Administration;
using ECommBackend.DTOs;
using ECommBackend.Models;
using Microsoft.Extensions.Options;
namespace ECommBackend.Services
{
    public class AzureServiceBusClass
    {
        public ServiceBusClient _serviceBusClient;
        public ServiceBusAdministrationClient _serviceBusAdministrationClient;
        public AzureServiceBusClass(IOptions<ServiceBusSettings> serviceBusSettings)
        {
            var connectionString = serviceBusSettings.Value.PrimaryConn;
            _serviceBusAdministrationClient = new ServiceBusAdministrationClient(connectionString);
            _serviceBusClient = new ServiceBusClient(connectionString);
           
        }

        public const string OrderQueueName = "userorderqueue";

        public static CreateQueueOptions OrderQueueOptions() => new(OrderQueueName)
        {
            MaxDeliveryCount = 5,
            LockDuration = TimeSpan.FromMinutes(2)
        };

        public const string EmailQueueName = "orderemailqueue";

        public static CreateQueueOptions EmailQueueOptions() => new(EmailQueueName)
        {
            MaxDeliveryCount = 5,
            LockDuration = TimeSpan.FromMinutes(1)
        };

        public async Task CreateOrder(OrderModel _orderModel)
        {

            if (!(await _serviceBusAdministrationClient.QueueExistsAsync(OrderQueueName))) {
                await _serviceBusAdministrationClient.CreateQueueAsync(OrderQueueOptions());
            }
            
            var msgSerializer = JsonConvert.SerializeObject(_orderModel);
            var messageSender = _serviceBusClient.CreateSender(OrderQueueName);
            var message = new ServiceBusMessage(msgSerializer);
            message.MessageId = _orderModel.OrderId.ToString();

            using var activity = MessageTracing.StartPublish(OrderTelemetry.Source, OrderQueueName, message.MessageId);
            activity?.SetTag("order.id", _orderModel.OrderId);
            MessageTracing.Inject(activity, message.ApplicationProperties);

            await messageSender.SendMessageAsync(message);
            OrderTelemetry.RecordPublished(OrderQueueName);
            await messageSender.CloseAsync();
        }

        public async Task PublishOrderEmail(OrderEmailMessage _emailMessage, CancellationToken ctx = default)
        {
            if (!(await _serviceBusAdministrationClient.QueueExistsAsync(EmailQueueName, ctx))) {
                await _serviceBusAdministrationClient.CreateQueueAsync(EmailQueueOptions(), ctx);
            }

            var msgSerializer = JsonConvert.SerializeObject(_emailMessage);
            await using var messageSender = _serviceBusClient.CreateSender(EmailQueueName);
            var message = new ServiceBusMessage(msgSerializer)
            {
                MessageId = $"{_emailMessage.OrderId}-{_emailMessage.OrderStatus}",
                ContentType = "application/json"
            };

            using var activity = MessageTracing.StartPublish(OrderTelemetry.Source, EmailQueueName, message.MessageId);
            activity?.SetTag("order.id", _emailMessage.OrderId);
            activity?.SetTag("order.status", _emailMessage.OrderStatus.ToString());
            MessageTracing.Inject(activity, message.ApplicationProperties);

            await messageSender.SendMessageAsync(message, ctx);
            OrderTelemetry.RecordPublished(EmailQueueName);
        }
    }
}

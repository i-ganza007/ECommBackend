using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus.Administration;
using ECommBackend.Models;
namespace ECommBackend.Services
{
    public class AzureServiceBusClass
    {
        private readonly IConfiguration _configuration;
        public ServiceBusClient _serviceBusClient;
        public ServiceBusAdministrationClient _serviceBusAdministrationClient;
        public AzureServiceBusClass(IConfiguration configuration)
        {
            _configuration = configuration;
            _serviceBusAdministrationClient = new ServiceBusAdministrationClient(_configuration["SecretKeys:PrimaryConn"]);
            _serviceBusClient = new ServiceBusClient(_configuration["SecretKeys:PrimaryConn"]) ;
           
        }

        public const string OrderQueueName = "userorderqueue";

        // Basic tier: sessions and duplicate detection aren't available.
        public static CreateQueueOptions OrderQueueOptions() => new(OrderQueueName)
        {
            MaxDeliveryCount = 5,
            LockDuration = TimeSpan.FromMinutes(2)
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
            await messageSender.SendMessageAsync(message);
            await messageSender.CloseAsync();
        }
    }
}

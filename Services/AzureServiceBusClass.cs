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
            _serviceBusClient = new ServiceBusClient(_configuration.GetSection("SecretKeys")["PrimaryConn"]) ;
            _serviceBusAdministrationClient = new ServiceBusAdministrationClient(_configuration.GetSection("SecretKeys")["PrimaryConn"]);
        }

        public async Task CreateOrder(OrderModel _orderModel)
        {

            if (!(await _serviceBusAdministrationClient.QueueExistsAsync("OrderPlaced"))) {
                await _serviceBusAdministrationClient.CreateQueueAsync(new CreateQueueOptions("OrderPlaced")
                {
                    RequiresSession = true,
                    RequiresDuplicateDetection = true,
                    MaxDeliveryCount = 5,
                    LockDuration = TimeSpan.FromMinutes(2)
                });
            }
            
            var msgSerializer = JsonConvert.SerializeObject(_orderModel);
            var messageSender = _serviceBusClient.CreateSender("OrderPlaced");
            var message = new ServiceBusMessage(msgSerializer);
            message.MessageId = _orderModel.OrderId.ToString();
            message.SessionId = _orderModel.Products.First(x=>x.Base_SKU is not null).Base_SKU; // Here this means that we'll process the orders one by one per variant and send multiple
            await messageSender.SendMessageAsync(message);
            await messageSender.CloseAsync();
        }
    }
}

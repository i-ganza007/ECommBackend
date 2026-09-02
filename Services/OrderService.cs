using ECommBackend.DTOs;
using ECommBackend.DTOs.MapToDomain;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;

namespace ECommBackend.Services
{
    public class OrderService
    {
        private readonly IOrderRepo _orderRepo;
        public OrderService(IOrderRepo orderRepo) { 
          _orderRepo = orderRepo;
        }

        public async Task<OrderDTO> GetSingleOrder(Guid _orderId, CancellationToken ctx) { 
          var result = await _orderRepo.GetSingleOrder(_orderId, ctx);
          return OrderMapToDomain.ModelToRecordDTO(result);
        }
        public async Task<IEnumerable<OrderDTO>?> GetAllOrders(Guid _userId, CancellationToken ctx) {
          var result = await _orderRepo.GetAllOrders(_userId, ctx);
          return result.Select(x=>OrderMapToDomain.ModelToRecordDTO(x));
        }

        public async Task CreateOrder(OrderModel order) {
           await _orderRepo.CreateOrder(order);
        }

        public async Task<UserDTO> GetOrderCreator(Guid _userId, CancellationToken ctx) { 
          var result = await _orderRepo.GetOrderCreator(_userId, ctx);
          return UserMapToDomain.ModelToRecordDTO(result);
        }
    }
}

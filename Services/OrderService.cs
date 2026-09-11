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
          return result.ModelToRecordDTO();
        }
        public async Task<IQueryable<OrderDTO>?> GetAllOrders(Guid _userId, CancellationToken ctx) {
          var result = await _orderRepo.GetAllOrders(_userId, ctx);
          return result.Select(x=>x.ModelToRecordDTO());
        }

        public async Task<Guid> CreateOrder(OrderModel order) {
           var result = await _orderRepo.CreateOrder(order);
            return result;
        }

        public async Task<UserDTO> GetOrderCreator(Guid _userId, CancellationToken ctx) { 
          var result = await _orderRepo.GetOrderCreator(_userId, ctx);
          return result.ModelToRecordDTO();
        }
    }
}

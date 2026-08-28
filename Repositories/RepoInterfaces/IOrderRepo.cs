using ECommBackend.Models;

namespace ECommBackend.Repositories.RepoInterfaces
{
    public interface IOrderRepo
    {
        public Task<OrderModel> GetSingleOrder(Guid _orderId, CancellationToken ctx);
        public Task<IEnumerable<OrderModel>?> GetAllOrders(Guid _userId, CancellationToken ctx);

        public Task CreateOrder(OrderModel order);

        public Task<UserModel> GetOrderCreator(Guid _userId, CancellationToken ctx);
    }
}

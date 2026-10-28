using ECommBackend.Models;
using ECommBackend.Models.ModInterfaces;

namespace ECommBackend.Repositories.RepoInterfaces
{
    public enum FinalizeResult
    {
        Processed,
        Rejected,
        Cancelled,
        AlreadyFinalized
    }

    public interface IOrderRepo
    {
        public Task<OrderModel> GetSingleOrder(Guid _orderId, CancellationToken ctx);
        public Task<IQueryable<OrderModel>?> GetAllOrders(Guid _userId, CancellationToken ctx);

        public Task<Guid> CreateOrder(OrderModel order, CancellationToken ctx);

        public Task UpdateOrder(Guid _orderId, OrderStatus status, CancellationToken ctx);

        public Task<long> CountPendingOrders(CancellationToken ctx);

        public Task<FinalizeResult> TryFinalizeOrder(Guid _orderId, IReadOnlyCollection<VariantReservation> reservations, CancellationToken ctx);

        public Task<OrderModel> CancelOrder(Guid _orderId, Guid _userId, CancellationToken ctx);


        public Task<UserModel> GetOrderCreator(Guid _userId, CancellationToken ctx);
    }
}

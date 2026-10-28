using ECommBackend.CustomErrors;
using ECommBackend.DatabaseConns;
using ECommBackend.Models;
using ECommBackend.Models.ModInterfaces;
using ECommBackend.Repositories.RepoInterfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommBackend.Repositories
{
    public class OrderRepo:IOrderRepo
    {
        private readonly SQLiteConn _SQLiteConn;
        public OrderRepo(SQLiteConn SQLiteConn) {
            _SQLiteConn = SQLiteConn;
        
        }
        public async Task<OrderModel> GetSingleOrder(Guid _orderId, CancellationToken ctx) { 
             var result = await _SQLiteConn.Orders.FirstOrDefaultAsync(x=>x.OrderId==_orderId,ctx);
            if (result == null) {
                throw new OrderNotFoundException($"Order {_orderId} can't be found ");
            }
            return result;
        }
        public async Task<IQueryable<OrderModel>?> GetAllOrders(Guid _userId, CancellationToken ctx) { 
        
            var result = await _SQLiteConn.Orders.ToListAsync(ctx);
            return result.AsQueryable();
        }

        

        public async Task UpdateOrder(Guid _orderId, OrderStatus status, CancellationToken ctx)
        {
            var result = await _SQLiteConn.Orders.FirstOrDefaultAsync(x=>x.OrderId == _orderId,ctx);
            if (result == null)
            {
                throw new OrderNotFoundException($"Order {_orderId} can't be found ");
            }
            result.OrderStatus = status;
            result.Version = Guid.NewGuid();
            await _SQLiteConn.SaveChangesAsync(ctx);
        }

        public Task<long> CountPendingOrders(CancellationToken ctx) =>
            _SQLiteConn.Orders.LongCountAsync(x => x.OrderStatus == OrderStatus.Pending, ctx);

        // Reserves stock and saves the final status in one transaction, so an order that lost the
        // race to a cancellation never keeps its stock decremented. An empty reservation set rejects the order.
        public async Task<FinalizeResult> TryFinalizeOrder(Guid _orderId, IReadOnlyCollection<VariantReservation> reservations, CancellationToken ctx)
        {
            await using var transaction = await _SQLiteConn.Database.BeginTransactionAsync(ctx);

            var order = await _SQLiteConn.Orders.FirstOrDefaultAsync(x => x.OrderId == _orderId, ctx);
            if (order == null)
            {
                throw new OrderNotFoundException(_orderId, $"Order {_orderId} can't be found ");
            }
            // The caller may already track this order from before the transaction began.
            await _SQLiteConn.Entry(order).ReloadAsync(ctx);

            if (order.OrderStatus != OrderStatus.Pending)
            {
                return ToNotPendingResult(order.OrderStatus);
            }

            var reserved = false;
            if (reservations.Count > 0)
            {
                await transaction.CreateSavepointAsync("reserve", ctx);
                reserved = await VariantReservations.TryApplyAsync(_SQLiteConn, reservations, ctx);
                if (!reserved)
                {
                    await transaction.RollbackToSavepointAsync("reserve", ctx);
                }
            }

            order.OrderStatus = reserved ? OrderStatus.Processed : OrderStatus.Rejected;
            order.Version = Guid.NewGuid();

            try
            {
                await _SQLiteConn.SaveChangesAsync(ctx);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(ctx);
                await _SQLiteConn.Entry(order).ReloadAsync(ctx);
                return ToNotPendingResult(order.OrderStatus);
            }

            await transaction.CommitAsync(ctx);
            return reserved ? FinalizeResult.Processed : FinalizeResult.Rejected;
        }

        private static FinalizeResult ToNotPendingResult(OrderStatus status) =>
            status == OrderStatus.Cancelled ? FinalizeResult.Cancelled : FinalizeResult.AlreadyFinalized;

        public async Task<OrderModel> CancelOrder(Guid _orderId, Guid _userId, CancellationToken ctx)
        {
            // Someone else's order is reported as missing so its existence isn't revealed.
            var order = await _SQLiteConn.Orders.FirstOrDefaultAsync(x => x.OrderId == _orderId && x.OrderCreatorId == _userId, ctx);
            if (order == null)
            {
                throw new OrderNotFoundException(_orderId, $"Order {_orderId} can't be found ");
            }

            if (order.OrderStatus != OrderStatus.Pending)
            {
                throw new OrderNotPendingError(_orderId, $"Order {_orderId} is {order.OrderStatus} and can no longer be cancelled");
            }

            order.OrderStatus = OrderStatus.Cancelled;
            order.Version = Guid.NewGuid();

            try
            {
                await _SQLiteConn.SaveChangesAsync(ctx);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new OrderNotPendingError(_orderId, $"Order {_orderId} was finalized before it could be cancelled", ex);
            }

            return order;
        }

        public async  Task<Guid> CreateOrder(OrderModel order, CancellationToken ctx) {
            await _SQLiteConn.Orders.AddAsync(order, ctx);
            await _SQLiteConn.SaveChangesAsync(ctx);
            return order.OrderId;
        }

        public async Task<UserModel> GetOrderCreator(Guid _userId, CancellationToken ctx) {
            var result = await _SQLiteConn.Orders.FirstOrDefaultAsync(x => x.OrderCreatorId == _userId, ctx);
            if (result == null)
            {
                throw new Exception($"OrderOwner {_SQLiteConn} can't be found ");
            }
            return result?.OrderCreator;

        }
    }
}

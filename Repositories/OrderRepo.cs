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
                throw new Exception($"Order {_orderId} can't be found ");
            }
            return result;
        }
        public async Task<IEnumerable<OrderModel>?> GetAllOrders(Guid _userId, CancellationToken ctx) { 
        
            var result = await _SQLiteConn.Orders.ToListAsync(ctx);
            return result;
        }

        public async  Task CreateOrder(OrderModel order) { }

        public async Task<UserModel> GetOrderCreator(Guid _userId, CancellationToken ctx) {
            var result = await _SQLiteConn.Orders.FirstAsync(x => x.OrderCreatorId == _userId, ctx);
            if (result == null)
            {
                throw new Exception($"OrderOwner {_SQLiteConn} can't be found ");
            }
            return result;

        }
    }
}

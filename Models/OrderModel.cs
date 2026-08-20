using ECommBackend.Models.ModInterfaces;
using System.Diagnostics.CodeAnalysis;

namespace ECommBackend.Models
{
    public class OrderModel: IOrder
    {
        public required Guid OrderId { get; set; }
        public required decimal TotalPrice { get; set; }
        public required UserModel OrderCreator { get; init; } // Will fail because of bind to other entities or tables 

        public required ProductModel[] Products { get; init; }
        public required DateTime CreatedDate { get; set; }
        public required OrderStatus OrderStatus { get; set; }


        [SetsRequiredMembers]
        public OrderModel(Guid orderId, decimal totalPrice,  DateTime createdDate, OrderStatus orderStatus) {
        OrderId = orderId;
            TotalPrice = totalPrice;
            OrderCreator = OrderCreator;
            CreatedDate = createdDate;
            OrderStatus = orderStatus;
        }
    }
}

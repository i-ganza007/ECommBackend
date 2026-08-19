using ECommBackend.Models.ModInterfaces;

namespace ECommBackend.Models
{
    public class OrderModel: IOrder
    {
        public required Guid OrderId { get; set; }
        public required decimal TotalPrice { get; set; }
        public required UserModel OrderCreator { get; set; }

        public required ProductModel[] Products { get; set; }
        public required DateTime CreatedDate { get; set; }
        public required OrderStatus OrderStatus { get; set; }


        public OrderModel(Guid _orderId,decimal _totalPrice,UserModel _orderCreator, ProductModel[] _products,DateTime _createdDate,OrderStatus _orderStatus) {
        OrderId = _orderId;
            TotalPrice = _totalPrice;
            OrderCreator = _orderCreator;
            Products = _products;
            CreatedDate = _createdDate;
            OrderStatus = _orderStatus;
        }
    }
}

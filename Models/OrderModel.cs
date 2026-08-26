using ECommBackend.Models.ModInterfaces;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;
namespace ECommBackend.Models
{
    public class OrderModel: IOrder
    {
        [Required]
        [Key]
        public required Guid OrderId { get; set; }
        [Required]
        [Range(1.00, double.MaxValue)]
        public required double TotalPrice { get; set; }
        [Required]
        public required UserModel OrderCreator { get; init; } // Will fail because of bind to other entities or tables 

        [Required]
        public required ICollection<ProductModel> Products { get; init; }
        [Required]
        public required DateTime CreatedDate { get; set; }
        [Required]
        public required OrderStatus OrderStatus { get; set; }


        [SetsRequiredMembers]
        public OrderModel(Guid orderId, double totalPrice, UserModel orderCreator, DateTime createdDate ,OrderStatus orderStatus) {
        OrderId = orderId;
            TotalPrice = totalPrice;
            OrderCreator = orderCreator;
            CreatedDate = createdDate;
            OrderStatus = orderStatus;
        }
    }
}

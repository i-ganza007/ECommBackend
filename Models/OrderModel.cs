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
        public  UserModel OrderCreator { get; set; } = null!; // Will fail because of bind to other entities or tables 

        [Required]
        public Guid OrderCreatorId { get; set; }

        [Required]
        public required ICollection<ProductModel> Products { get; init; } = new List<ProductModel>();
        [Required]
        public required DateTime CreatedDate { get; set; }
        [Required]
        public required OrderStatus OrderStatus { get; set; }

        [SetsRequiredMembers]
        public OrderModel(Guid orderId, double totalPrice, Guid orderCreatorId, DateTime createdDate ,OrderStatus orderStatus) {
        OrderId = orderId;
            TotalPrice = totalPrice;
            OrderCreatorId = orderCreatorId;
            CreatedDate = createdDate;
            OrderStatus = orderStatus;
        }
    }
}

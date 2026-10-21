using ECommBackend.Models.ModInterfaces;

namespace ECommBackend.DTOs
{
   
    public record OrderEmailMessage(
        Guid OrderId,
        string ToEmail,
        string ToName,
        OrderStatus OrderStatus,
        double TotalPrice,
        DateTime CreatedDate);
}

namespace ConsumerProject.Models;

public enum OrderStatus
{
    Pending,
    Processed,
    Rejected,
    Cancelled
}

public record OrderEmailMessage(
    Guid OrderId,
    string ToEmail,
    string ToName,
    OrderStatus OrderStatus,
    double TotalPrice,
    DateTime CreatedDate);

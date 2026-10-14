namespace ConsumerProject.Models;

public enum OrderStatus
{
    Pending,
    Processed,
    Rejected
}

public record OrderEmailMessage(
    Guid OrderId,
    string ToEmail,
    string ToName,
    OrderStatus OrderStatus,
    double TotalPrice,
    DateTime CreatedDate);

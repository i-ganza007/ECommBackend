using ECommBackend.Models.ModInterfaces;

namespace ECommBackend.DTOs
{
    // OrderDTO without navigations, safe to return when OrderCreator/Products aren't loaded.
    public record OrderSummaryDTO(Guid _OrderId, double _TotalPrice, Guid _OrderCreatorId, DateTime _CreatedDate, OrderStatus _OrderStatus)
    {
    }
}

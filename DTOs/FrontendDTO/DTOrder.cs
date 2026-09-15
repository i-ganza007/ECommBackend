using ECommBackend.Models.ModInterfaces;

namespace ECommBackend.DTOs.FrontendDTO
{
    public record DTOrder(double _TotalPrice,  Guid _OrderCreatorId, ICollection<String> _ProductsIds, DateTime _CreatedDate, OrderStatus _OrderStatus)
    {
    }
}

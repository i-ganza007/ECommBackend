using ECommBackend.Models.ModInterfaces;

namespace ECommBackend.DTOs.FrontendDTO
{
    public record DTOrder(double _TotalPrice, DTOUser _OrderCreator, Guid _OrderCreatorId, ICollection<DTOProduct> _Products, DateTime _CreatedDate, OrderStatus _OrderStatus)
    {
    }
}

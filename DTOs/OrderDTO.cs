using ECommBackend.Models.ModInterfaces;

namespace ECommBackend.DTOs
{
    public record OrderDTO(Guid _OrderId,double _TotalPrice,UserDTO _OrderCreator,Guid _OrderCreatorId,ICollection<ProductDTO> _Products,DateTime _CreatedDate,OrderStatus _OrderStatus)
    {
    }
}

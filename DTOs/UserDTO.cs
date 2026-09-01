using ECommBackend.Models;

namespace ECommBackend.DTOs
{
    public record UserDTO(Guid _UserId,string _FirstName,string _LastName,string _Email,ICollection<ProductDTO> _ProductsBought,DateTime _CreatedAt,ICollection<OrderDTO> _Orders)
    {
    }
}

using ECommBackend.Models;

namespace ECommBackend.DTOs
{
    public record AdminDTO(Guid _UserId, string _FirstName, string _LastName, string _Email, ICollection<ProductDTO> _ProductsOwned, DateTime _CreatedAt)
    {
    }
}

using ECommBackend.Models;
using ECommBackend.Models.ModInterfaces;

namespace ECommBackend.DTOs.FrontendDTO
{
    public record DTOProduct(string _Name, string _Description, ProductCategory _Category, ICollection<DTOVariant> _Variants, string _Base_SKU, string? _Texture, string? _Skin_Type, string? _Key_Ingr, Guid _AdminOwnerId, DTOAdmin _Owner)
    {
    }
}

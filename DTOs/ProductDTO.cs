using ECommBackend.Models;
using ECommBackend.Models.ModInterfaces;

namespace ECommBackend.DTOs
{
    public record ProductDTO(Guid _ProductId,string _Name,string _Description,ProductCategory _Category,ICollection<VariantModel> _Variants,string _Base_SKU,string? _Texture,string? _Skin_Type,string? _Key_Ingr,DateTime _CreatedAt,Guid _AdminOwnerId,AdminModel _Owner)
    {
    }
}

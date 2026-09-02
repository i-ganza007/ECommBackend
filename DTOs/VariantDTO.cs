using ECommBackend.Models;

namespace ECommBackend.DTOs
{
    public record VariantDTO(Guid _VariantId,double _Size,decimal _Price,int _Units,Guid _VariantImageId,ImageDTO _Image,ProductDTO _Product)
    {
    }
}

namespace ECommBackend.DTOs.FrontendDTO
{
    public record DTOVariant(double _Size, decimal _Price, int _Units, Guid _VariantImageId,  Guid _productId)
    {
    }
}

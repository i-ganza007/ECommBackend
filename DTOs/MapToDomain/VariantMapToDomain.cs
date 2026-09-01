using ECommBackend.Models;

namespace ECommBackend.DTOs.MapToDomain
{
    public static class VariantMapToDomain
    {
        public static VariantDTO ModelToRecordDTO(VariantModel variantModel)
        {
            return new VariantDTO(
                variantModel.VariantId,
                variantModel.Size,
                variantModel.Price,
                variantModel.Units,
                variantModel.VariantImageId,
                ImageMapToDomain.ModelToRecordDTO(variantModel.VariantImage),
                ProductMapToDomain.ModelToRecordDTO(variantModel.Product)
                );
        }
    }
}

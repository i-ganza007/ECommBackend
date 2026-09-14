using ECommBackend.Models;

namespace ECommBackend.DTOs.MapToDomain
{
    public static class VariantMapToDomain
    {
        public static VariantDTO ModelToRecordDTO(this VariantModel variantModel)
        {
            return new VariantDTO(
                variantModel.VariantId,
                variantModel.Size,
                variantModel.Price,
                variantModel.Units,
                variantModel.VariantImageId,
                variantModel.VariantImage.ModelToRecordDTO(),
                variantModel.Product.ModelToRecordDTO()
                );
        }
    }
}

using ECommBackend.Models;

namespace ECommBackend.DTOs.MapToDomain
{
    public static class ProductMapToDomain
    {
        public static ProductDTO ModelToRecordDTO(ProductModel _productModel) {
            return new ProductDTO(
                _productModel.ProductId,
                _productModel.Name,
                _productModel.Description,
                _productModel.Category,
                _productModel.Variants,
                _productModel.Base_SKU,
                _productModel.Texture,
                _productModel.Skin_Type,
                _productModel.Key_Ingr,
                _productModel.CreatedAt,
                _productModel.AdminOwnerId,
                _productModel.Owner



                );
        }
    }
}

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ECommBackend.Models
{
    public class VariantModel
    {
        [Key]
        public Guid VariantId { get; set; }

        [Range(0.01, double.MaxValue)]
        public double Size { get; set; }

        public decimal Price { get; set; }

        [Range(1, int.MaxValue)]
        public int Units { get; set; }

        public Guid VariantImageId { get; set; }

        public ImageModel VariantImage { get; set; } = null!;

        public Guid ProductModelId { get; set; }

        public ProductModel Product { get; set; } = null!;

        public VariantModel()
        {
        }

        public VariantModel(
            Guid variantId,
            double size,
            decimal price,
            Guid productModelId,
            int units,
            Guid variantImageId)
        {
            VariantId = variantId;
            Size = size;
            Price = price;
            ProductModelId = productModelId;
            Units = units;
            VariantImageId = variantImageId;
        }

        public void Deconstruct(out Guid variantId, out double size, out decimal price, out Guid productModelId, out int units, out Guid variantImageId) 
        { variantId = VariantId; size = Size; price = Price; productModelId = ProductModelId; units = Units; variantImageId = VariantImageId; }
    }
}

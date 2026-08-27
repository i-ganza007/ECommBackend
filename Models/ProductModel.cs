using ECommBackend.Models.ModInterfaces;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;


namespace ECommBackend.Models
{
    public class ProductModel: IProduct
    {

        [Key]
        public required Guid ProductId { get; set; }

        [Required]
        [Length(5, 30)]
        public required string Name { get; set; }

        [Required]
        [Length(10, 30)]
        public required string Description { get; set; }

        [Required]
        public ProductCategory Category { get; set; }


        [Required]
        
        public  ICollection<VariantModel> Variants { get; set; } = new List<VariantModel>();


        [Required]
        public required string Base_SKU { get; set; }

      

        
        public string? Texture { get; set; }

        public string? Skin_Type { get; set; }

        public string? Key_Ingr { get; set; }

        // It's not advised to make these private because c# think that you're trying to initialise it from the outside yet it's private

        public  DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdateAt { get; set; }

        [Required]
        public Guid AdminOwnerId { get; set; }
        public  AdminModel Owner { get; set; } = null!;

        [SetsRequiredMembers]
        public ProductModel(Guid productId, string base_SKU, string name,  string description, Guid adminOwnerId) {
        ProductId = productId;
        Name = name;
        Description = description;
        Base_SKU = base_SKU;
        AdminOwnerId = adminOwnerId;
        }


    }
}

using System.ComponentModel.DataAnnotations;
namespace ECommBackend.Models.ModInterfaces
{
    public enum ProductCategory
    {
        Body,
        Cleansers,
       Hands,
            Masks,
            Moisturisers,
            Serums,
            Sun_Care
    };
    public interface IProduct
    {

        
        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ProductCategory Category { get; set; }


        public string Base_SKU { get; set; }


        public string? Texture { get; set; }

        public string? Skin_Type { get; set;  }

        public string? Key_Ingr { get; set;  }

        public List<VariantModel> Variants { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdateAt { get; set; }
        public AdminModel Owner { get; init;  }
    }
}

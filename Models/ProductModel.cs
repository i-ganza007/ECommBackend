using ECommBackend.Models.ModInterfaces;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ECommBackend.Models
{
    public class ProductModel: IProduct
    {
        public required Guid ProductId { get; set; }
      
        public required string Name { get; set; }
       
        public required string Description { get; set; }
        
        public  decimal Price { get; private set; } // It's not advised to make these private because c# think that you're trying to initialise it from the outside yet it's private
     
        public required DateTime CreatedAt { get; set; }

        public DateTime? UpdateAt { get; set; }
      
        public required AdminModel Owner { get; init; }

        [SetsRequiredMembers]
        public ProductModel(Guid productId, string name, string description, decimal price, DateTime createdAt) {
        ProductId = productId;
        Name = name;
        Description = description;
        Price = price;
        CreatedAt = createdAt;
            Price = price;
        }

        public decimal PriceChanger(decimal newPrice) { 
            Price = newPrice;
            return Price;
        }
    }
}

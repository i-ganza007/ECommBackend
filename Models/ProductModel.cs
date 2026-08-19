using ECommBackend.Models.ModInterfaces;
using System.ComponentModel.DataAnnotations;

namespace ECommBackend.Models
{
    public class ProductModel: IProduct
    {
        public required Guid ProductId { get; set; }
      
        public required string Name { get; set; }
       
        public required string Description { get; set; }
        
        private  decimal Price { get; set; }
     
        public required DateTime CreatedAt { get; set; }

        public DateTime? UpdateAt { get; set; }
      
        public required AdminModel Owner { get; set; }

        public ProductModel(Guid _productId,string _name,string _description,decimal _price,DateTime _createdAt,AdminModel _owner) {
        ProductId = _productId;
        Name = _name;
        Description = _description;
        Price = _price;
        CreatedAt = _createdAt;
        Owner = _owner;
          
        }

        public decimal PriceChanger(decimal newPrice) { 
            Price = newPrice;
            return Price;
        }
    }
}

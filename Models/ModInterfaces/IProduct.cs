using System.ComponentModel.DataAnnotations;
namespace ECommBackend.Models.ModInterfaces
{
    public interface IProduct
    {
        [Key]
        
        public Guid ProductId { get; set; }
        [Required]
        [Length(5,30)]
        public string Name { get; set; }
        [Required]
        [Length(10, 30)]
        public string Description { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdateAt { get; set; }
        [Required]
        public AdminModel Owner { get; set;  }
    }
}

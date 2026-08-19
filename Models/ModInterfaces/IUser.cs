using System.ComponentModel.DataAnnotations;
namespace ECommBackend.Models.ModInterfaces
{
    public interface IUser
    {
        [Key]
        public Guid userId { get; set; }
        [Required]
        [Length(3, 10)]
        public string FirstName { get; set; }
        [Required]
        [Length(3,10)]
        public string LastName { get; set; }
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Range(18,100)]
        [Required]
        public int Age { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public DateTime? UpdatedAt { get; set; }

    }
}

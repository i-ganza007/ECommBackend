using SydAnnotations =  System.ComponentModel.DataAnnotations;
namespace ECommBackend.Models.ModInterfaces
{
    public interface IUser
    {
        [SydAnnotations.Key]
        public Guid UserId { get; set; }
        [SydAnnotations.Required]
        [SydAnnotations.Length(3, 10)]
        public string FirstName { get; set; }
        [SydAnnotations.Required]
        [SydAnnotations.Length(3,10)]
        public string LastName { get; set; }
        [SydAnnotations.EmailAddress]
        [SydAnnotations.Required]
        public string Email { get; set; }
        [SydAnnotations.Range(18,100)]
        [SydAnnotations.Required]
        public int Age { get; set; }
        [SydAnnotations.Required]
        public DateTime CreatedDate { get; set; }
        [SydAnnotations.Required]
        public DateTime? UpdatedAt { get; set; }

    }
}

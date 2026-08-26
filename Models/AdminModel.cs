using ECommBackend.Models.ModInterfaces;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ECommBackend.Models
{
    public class AdminModel:IUser
    {
        [Key]

        public required Guid UserId { get; set; }
        [Required]
        [Length(3, 10)]
        public required string FirstName { get; set; }
        [Required]
        [Length(3, 10)]
        public required string LastName { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        [Range(18, 100)]
        public required int Age { get; set; }
        [Required]
        public string Password { get; private set; }

        public string RefreshToken { get; private set; }

        public ICollection<ProductModel> ProductsOwned { get; set; } = new List<ProductModel>();
        [Required]
        public required DateTime CreatedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [SetsRequiredMembers] // Because compiler doesn't trust you to initialise the required properties so need this, If not complaints
        public AdminModel(Guid userId, string firstName, string lastName, string email, int age, string password, string refreshToken)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Age = age;
            Password = password;
            RefreshToken = refreshToken;
        }

        public void ChangePassword(string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("Password cannot be empty");
            }
            if(Password == newPassword)
            {
                throw new ArgumentException("Cannot have the same password must have a new one");
            }
            Password = newPassword;
        }

        public string RefreshTokenChanger(string newRefreshToken)
        {
            RefreshToken = newRefreshToken;
            return RefreshToken;
        }
    }
}

using ECommBackend.Models.ModInterfaces;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
namespace ECommBackend.Models
{
    public class UserModel:IUser
    {
        public required Guid UserId { get; set; }
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }
        public required int Age { get; set; }
        public string Password { get; private set; }
        public string RefreshToken { get; private set; }

        public ProductModel[]? ProductsBought { get; set; }
        public required DateTime CreatedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public OrderModel[]? OrderOrders { get; set; } 

        [SetsRequiredMembers] // Because compiler doesn't trust you to initialise the required properties so need this, If not complaints
        public UserModel(Guid userId, string firstName, string lastName, string email, int age, string password, string refreshToken, DateTime createdDate)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            CreatedDate = createdDate;
            Email = email;
            Age = age;
            Password = password;
            RefreshToken = refreshToken;
        }

        public string PasswordChanger(string password)
        {
            Password = password;
            return Password;
        }

        public string RefreshTokenChanger(string newRefreshToken)
        {
            RefreshToken = newRefreshToken;
            return RefreshToken;
        }
    }
}

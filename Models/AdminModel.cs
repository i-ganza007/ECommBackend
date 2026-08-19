using ECommBackend.Models.ModInterfaces;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ECommBackend.Models
{
    public class AdminModel:IUser
    {
        public required Guid userId { get; set; }
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }
        public required int Age { get; set; }
        private string Password { get; set; }
        private string RefreshToken { get; set; }

        public ProductModel[]? ProductOwned { get; set; }
        public required DateTime CreatedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [SetsRequiredMembers] // Because compiler doesn't trust you to initialise the required properties so need this, If not complaints
        public AdminModel(Guid _userId, string _firstName, string _lastName, string _email, int _age, string _password, string _refreshToken,DateTime _createdTime)
        {
            userId = _userId;
            FirstName = _firstName;
            LastName = _lastName;
            CreatedDate = _createdTime;
            Email = _email;
            Age = _age;
            Password = _password;
            RefreshToken = _refreshToken;
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

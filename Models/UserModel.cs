using ECommBackend.Models.ModInterfaces;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
namespace ECommBackend.Models
{
    public class UserModel:IUser
    {
        public required Guid userId { get; set; }
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }
        public required int Age { get; set; }
        private  string Password { get; set; }
        private string RefreshToken { get; set; }

        public required DateTime CreatedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [SetsRequiredMembers]
        public UserModel(Guid _userId, string _firstName, string _lastName, string _email, int _age, string _password,string _refreshToken, DateTime _createdTime)
        {
            userId = _userId;
            FirstName = _firstName;
            LastName = _lastName;
            Email = _email;
            CreatedDate = _createdTime;
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

using ECommBackend.Models;
using System.Security.Claims;

namespace ECommBackend.Services.IJWTServices
{
    public interface IJWTService
    {
        public string GenerateAccessToken(UserModel userModel);
        public string GenerateRefreshToken();
        public ClaimsPrincipal ValidateAccessToken(string token, bool validateLifeTime = true);
        public ClaimsPrincipal ValidateExpiredAccessToken(string token);
    }
}

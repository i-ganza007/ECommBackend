using ECommBackend.Models;
using ECommBackend.Models.ModInterfaces;
using System.Security.Claims;

namespace ECommBackend.Services.IJWTServices
{
    public enum Roles
    {
        free_user,
        admin,
        logged_in

    }
    public interface IJWTService
    {
        // IUser rather than UserModel: AdminModel does not derive from UserModel, so a
        // UserModel parameter left admins with no way to be issued a token.
        public string GenerateAccessToken(IUser userModel,Roles role);
        public string GenerateRefreshToken();
        public ClaimsPrincipal ValidateAccessToken(string token, bool validateLifeTime = true);
        public ClaimsPrincipal ValidateExpiredAccessToken(string token);


    }
}

using ECommBackend.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Experimental;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IjwtServices = ECommBackend.Services.IJWTServices ;
namespace ECommBackend.Services
{
    public class JwtServiceAuth: IjwtServices.IJWTService
    {
        private readonly JwtSettings _jwtSettings;
        public JwtServiceAuth(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateAccessToken(UserModel userModel) {
            var token = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var claims = new List<Claim> {
              new Claim(ClaimTypes.Email, userModel.Email),
              new Claim(ClaimTypes.Role,"user"),
              new Claim(ClaimTypes.NameIdentifier,userModel.UserId.ToString())
            };

            var secTokenDesc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessKeyTimeLineMinutes),
                Issuer=_jwtSettings.Issuer,
                Audience=_jwtSettings.Audience,
                SigningCredentials=new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.Sha256)
            };

            var tokenI = token.CreateToken(secTokenDesc);
            return token.WriteToken(tokenI);
        }
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        public ClaimsPrincipal ValidateAccessToken(string jwtToken, bool validateLifeTime = true) {
            var token = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            try
            {
                var tokenParams = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = validateLifeTime,
                    ClockSkew = TimeSpan.FromMinutes(1)

                };
                var decoded_token_principal = token.ValidateToken(jwtToken, tokenParams, out SecurityToken decoded);
                if (decoded is JwtSecurityToken secToken && !secToken.Header.Alg.Equals(SecurityAlgorithms.Sha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
                return decoded_token_principal;
)
            }
            catch (SecurityTokenException ex)
            {
                return null;
            }
        }
        public ClaimsPrincipal ValidateExpiredAccessToken(string token) {
            return ValidateAccessToken(token,false);
        }
    }
}

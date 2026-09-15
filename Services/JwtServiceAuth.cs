using ECommBackend.Models;
using ECommBackend.Models.ModInterfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
        private readonly ILogger _logger;
        public JwtServiceAuth(IOptions<JwtSettings> jwtSettings,ILogger<JwtServiceAuth> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public string GenerateAccessToken(IUser userModel,IjwtServices.Roles roles) {
            var token = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var claims = new List<Claim> {
              new Claim(ClaimTypes.Email, userModel.Email),
              new Claim(ClaimTypes.Role,roles.ToString()),
              new Claim(ClaimTypes.NameIdentifier,userModel.UserId.ToString())
            };

            var secTokenDesc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessKeyTimeLineMinutes),
                Issuer=_jwtSettings.Issuer,
                Audience=_jwtSettings.Audience,
                // HmacSha256, not Sha256: the latter names a digest, not a signature algorithm.
                SigningCredentials=new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256)
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
                if (decoded is JwtSecurityToken secToken && !secToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return decoded_token_principal;


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

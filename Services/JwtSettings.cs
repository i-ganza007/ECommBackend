using 
namespace ECommBackend.Services
{
    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public string RefreshKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }

        public int AccessKeyTimeLineMinutes { get; set; } = 15;
        public int RefreshKeyTimeLineDays { get; set; } = 7;
    }
}

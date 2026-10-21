using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ECommBackend.Services
{
    public static class AuthTelemetry
    {
        public const string Name = "ECommBackend.Auth";

        private static readonly Meter Meter = new(Name);

        private static readonly Counter<long> Logins = Meter.CreateCounter<long>(
            "ecomm.auth.logins", "{attempt}", "Login attempts, by role and outcome");

        private static readonly Counter<long> Registrations = Meter.CreateCounter<long>(
            "ecomm.auth.registrations", "{account}", "Accounts created, by role");

        public static void RecordLogin(string role, bool succeeded)
        {
            var outcome = succeeded ? "success" : "failure";
            Logins.Add(1, new TagList { { "auth.role", role }, { "auth.outcome", outcome } });
            Activity.Current?.SetTag("auth.outcome", outcome);
        }

        public static void RecordRegistration(string role) =>
            Registrations.Add(1, new KeyValuePair<string, object?>("auth.role", role));
    }
}

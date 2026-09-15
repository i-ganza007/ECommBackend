namespace ECommBackend.Services
{
    // Single place the bcrypt work factor and verification live, so no registration or login
    // path can quietly disagree with another about how passwords are stored.
    public static class PasswordHasher
    {
        private const int WorkFactor = 12;

        // Verifying a wrong password against a throwaway hash costs the same as verifying a real one.
        // Without it, an unknown email would return noticeably faster and leak which accounts exist.
        private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword("not-a-real-password", WorkFactor);

        public static string Hash(string plainTextPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword))
            {
                throw new ArgumentException("Password cannot be empty");
            }

            return BCrypt.Net.BCrypt.HashPassword(plainTextPassword, WorkFactor);
        }

        public static bool Verify(string? plainTextPassword, string? storedHash)
        {
            if (string.IsNullOrEmpty(plainTextPassword))
            {
                return false;
            }

            try
            {
                var matches = BCrypt.Net.BCrypt.Verify(plainTextPassword, storedHash ?? DummyHash);
                return matches && storedHash is not null;
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // A row whose password predates hashing (or was written by hand) is not a valid login.
                return false;
            }
        }
    }
}

using MailKit.Security;

namespace ConsumerProject.Services;

public class SmtpSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public SecureSocketOptions Security { get; set; } = SecureSocketOptions.None;

    public string? Username { get; set; }
    public string? Password { get; set; }

    public string FromName { get; set; } = "ECommShop";
    public string FromEmail { get; set; } = "no-reply@ecommshop.local";
}

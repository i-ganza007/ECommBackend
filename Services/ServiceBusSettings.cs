using System.ComponentModel.DataAnnotations;

namespace ECommBackend.Services;

public sealed class ServiceBusSettings
{
    [Required]
    public string PrimaryConn { get; set; } = string.Empty;
}

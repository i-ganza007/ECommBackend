using System.ComponentModel.DataAnnotations;

namespace ConsumerProject.Services;

public sealed class ServiceBusSettings
{
    [Required]
    public string PrimaryConn { get; set; } = string.Empty;
}

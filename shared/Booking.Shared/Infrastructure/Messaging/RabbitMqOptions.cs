namespace Booking.Shared.Infrastructure.Messaging;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    
    public string HostName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int Port { get; set; } 
    public string Password { get; set; } = string.Empty;
    public string VirtualHost { get; set; } = "/";
}
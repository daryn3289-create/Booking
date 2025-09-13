using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Booking.Shared.Api;

public static class LoggerExtension
{
    public static void AddLogger(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog();
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .CreateLogger();
    }
}
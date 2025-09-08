using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Booking.Shared.Infrastructure.Messaging;

public static class RabbitMqExtension
{
    public static void AddRabbitMq(this IServiceCollection services)
    {
        services.Configure<RabbitMqOptions>(
            options => services.BuildServiceProvider().GetRequiredService<IConfiguration>()
                .GetSection(RabbitMqOptions.SectionName).Bind(options));
        
        services.AddSingleton<IRabbitMqFactory, RabbitMqFactory>();
    }
}
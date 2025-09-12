using Booking.Shared.Api;
using Booking.Shared.Identity.Generated;
using Booking.Shared.Identity.Options;
using Booking.Shared.Infrastructure.Messaging;
using Identity.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Application;

public static class Extension
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddRabbitMq();
        
        services.AddHandlers(AppDomain.CurrentDomain.GetAssemblies());
        
        services.AddScoped<IKeycloakAdminTokenService, KeycloakAdminTokenService>();
        
        services.AddScoped<KeycloakHttpAdminInterceptor>();
        
        var options = services.BuildServiceProvider().GetRequiredService<IConfiguration>()
            .GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>() ?? throw new InvalidOperationException("KeycloakOptions not found");
        
        services.AddHttpClient<IKeycloakGeneratedExternalApiClient, KeycloakGeneratedExternalApiClient>(client =>
            {
                client.BaseAddress = new Uri(options.BaseAddress);
            })
            .AddHttpMessageHandler<KeycloakHttpAdminInterceptor>();

        services.AddHttpClient<IKeycloakAuthClient, KeycloakAuthClient>()
            .AddHttpMessageHandler<KeycloakHttpAdminInterceptor>();
    }
}
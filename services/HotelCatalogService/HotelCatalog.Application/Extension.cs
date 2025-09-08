using Booking.Shared.Api;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace HotelCatalog.Application;

public static class Extension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddHandlers(AppDomain.CurrentDomain.GetAssemblies());
        services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
        
        return services;
    }
}
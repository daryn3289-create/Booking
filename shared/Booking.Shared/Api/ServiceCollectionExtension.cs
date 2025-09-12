using System.Reflection;
using Booking.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Booking.Shared.Api;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует все обработчики из указанной сборки.
    /// </summary>
    public static void AddHandlers(this IServiceCollection services,
        Assembly[] assemblies)
    {
        var handlerInterfaces = new[]
        {
            typeof(ICommandHandler<>),
            typeof(ICommandHandler<,>),
            typeof(IQueryHandler<>),
            typeof(IQueryHandler<,>)
        };

        assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && handlerInterfaces.Contains(i.GetGenericTypeDefinition())))
            .ToList()
            .ForEach(services.TryAddScoped);
    }
}

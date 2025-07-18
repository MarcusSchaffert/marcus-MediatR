using System;
using System.Linq;
using System.Reflection;
using MediatR.Simple;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions to scan for MediatR.Simple handlers and register them.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers handlers and mediator types from the specified assemblies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assemblies">Assemblies to scan for handlers</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediatRSimple(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.", nameof(assemblies));
        }

        // Register the core services
        services.TryAddTransient<IMediator, Mediator>();
        services.TryAddTransient<ISender>(serviceProvider => serviceProvider.GetRequiredService<IMediator>());

        // Scan and register request handlers
        RegisterRequestHandlers(services, assemblies);

        return services;
    }

    /// <summary>
    /// Registers handlers and mediator types from the assembly containing the specified type
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <typeparam name="T">Type from assembly to scan</typeparam>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediatRSimple<T>(this IServiceCollection services)
    {
        return services.AddMediatRSimple(typeof(T).Assembly);
    }

    /// <summary>
    /// Registers handlers and mediator types from the assembly containing the specified type
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="type">Type from assembly to scan</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediatRSimple(this IServiceCollection services, Type type)
    {
        return services.AddMediatRSimple(type.Assembly);
    }

    private static void RegisterRequestHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        var handlerTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => type.GetInterfaces().Any(i => IsRequestHandlerInterface(i)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var handlerInterfaces = handlerType.GetInterfaces()
                .Where(IsRequestHandlerInterface)
                .ToList();

            foreach (var handlerInterface in handlerInterfaces)
            {
                services.AddTransient(handlerInterface, handlerType);
            }
        }
    }

    private static bool IsRequestHandlerInterface(Type type)
    {
        if (!type.IsGenericType) return false;

        var genericType = type.GetGenericTypeDefinition();
        return genericType == typeof(IRequestHandler<>) || genericType == typeof(IRequestHandler<,>);
    }
}

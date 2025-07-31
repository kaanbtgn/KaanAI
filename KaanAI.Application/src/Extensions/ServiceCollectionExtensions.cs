using KaanAI.Application.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace KaanAI.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        try
        {
            // Get the assembly containing the service implementations
            var applicationAssembly = typeof(ServiceCollectionExtensions).Assembly;
            
            // Find all types that implement IService
            var serviceTypes = applicationAssembly.GetTypes()
                .Where(type => !type.IsAbstract && !type.IsInterface && typeof(IService).IsAssignableFrom(type))
                .ToList();

            foreach (var serviceType in serviceTypes)
            {
                // Find the interface that this service implements (should be the one that extends IService)
                var serviceInterface = serviceType.GetInterfaces()
                    .FirstOrDefault(interfaceType => 
                        interfaceType != typeof(IService) && 
                        typeof(IService).IsAssignableFrom(interfaceType));

                if (serviceInterface != null)
                {
                    // Register the service with its interface
                    services.AddScoped(serviceInterface, serviceType);
                    
                    // Log the registration (optional, for debugging)
                    Console.WriteLine($"Registered service: {serviceInterface.Name} -> {serviceType.Name}");
                }
                else
                {
                    Console.WriteLine($"Warning: Service {serviceType.Name} implements IService but no specific interface found");
                }
            }

            return services;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during service registration: {ex.Message}");
            throw;
        }
    }
} 
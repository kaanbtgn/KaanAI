using KaanAI.Application.Abstraction;
using KaanAI.Application.Abstraction.Chat;
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

            var registeredServices = new List<string>();

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
                    registeredServices.Add($"{serviceInterface.Name} -> {serviceType.Name}");
                }
                else
                {
                    Console.WriteLine($"Warning: Service {serviceType.Name} implements IService but no specific interface found");
                }
            }

            // Log all registered services
            if (registeredServices.Any())
            {
                Console.WriteLine("=== Application Services Registered ===");
                foreach (var service in registeredServices)
                {
                    Console.WriteLine($"✓ {service}");
                }
                Console.WriteLine("=======================================");
            }

            return services;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during service registration: {ex.Message}");
            throw;
        }
    }

    public static void VerifyServiceRegistration(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        
        try
        {
            // Try to resolve IChatService to verify registration
            var chatService = serviceProvider.GetService<IChatService>();
            if (chatService != null)
            {
                Console.WriteLine("✓ IChatService successfully registered and resolved");
            }
            else
            {
                Console.WriteLine("✗ IChatService not found in service collection");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error resolving IChatService: {ex.Message}");
        }
    }
} 
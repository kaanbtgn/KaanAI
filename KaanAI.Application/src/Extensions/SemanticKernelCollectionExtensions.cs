using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using KaanAI.Application.Plugins;
using KaanAI.Application.Abstraction.SemanticKernel;
using KaanAI.Application.Abstraction;

namespace KaanAI.Application.Extensions;

public static class SemanticKernelCollectionExtensions
{
    /// <summary>
    /// Adds Semantic Kernel services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddSemanticKernel(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Get Azure OpenAI configuration
        var endpoint = configuration["AzureOpenAI:Endpoint"] ??
                      throw new ArgumentNullException("AzureOpenAI:Endpoint", "Azure OpenAI endpoint not configured");
        
        var apiKey = configuration["AzureOpenAI:APIKey"] ??
                    throw new ArgumentNullException("AzureOpenAI:APIKey", "Azure OpenAI API key not configured");
        
        var deploymentName = configuration["AzureOpenAI:DeploymentName"] ??
                            throw new ArgumentNullException("AzureOpenAI:DeploymentName", "Azure OpenAI deployment name not configured");

        // Add Semantic Kernel with Azure OpenAI
        services.AddKernel()
            .AddAzureOpenAIChatCompletion(
                deploymentName: deploymentName,
                endpoint: endpoint,
                apiKey: apiKey,
                apiVersion: configuration["AzureOpenAI:ApiVersion"] ?? "2024-06-01-preview");

        // Note: WeatherPlugin is manually instantiated in SemanticKernelService to avoid circular dependency
        // Note: SemanticKernelService should be automatically registered by AddApplicationServices()
        // since it implements ISemanticKernelService which extends IService
        
        return services;
    }
}
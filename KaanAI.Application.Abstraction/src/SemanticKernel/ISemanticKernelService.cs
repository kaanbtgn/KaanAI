using KaanAI.Application.Abstraction;
using KaanAI.Application.Abstraction.SemanticKernel.Contracts;

namespace KaanAI.Application.Abstraction.SemanticKernel;

public interface ISemanticKernelService : IService
{
    /// <summary>
    /// Main orchestrator method that analyzes user intent and routes to weather plugin
    /// </summary>
    /// <param name="request">Semantic Kernel request with message and configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Semantic Kernel response with weather plugin result and metadata</returns>
    Task<SemanticKernelResponseDto> ExecuteAsync(SemanticKernelRequestDto request, CancellationToken cancellationToken = default);
}
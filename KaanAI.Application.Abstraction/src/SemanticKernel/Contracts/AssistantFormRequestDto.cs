using Microsoft.AspNetCore.Http;

namespace KaanAI.Application.Abstraction.SemanticKernel.Contracts;

public class AssistantFormRequestDto
{
    public string Message { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public bool IncludeHistory { get; set; } = true;
    public string? PreferredPlugin { get; set; }
    public bool AutoDetectIntent { get; set; } = true;
    public List<IFormFile>? Files { get; set; }
}



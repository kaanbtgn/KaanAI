using System.ComponentModel.DataAnnotations;

namespace KaanAI.Application.Abstraction.SemanticKernel.Contracts;

public class SemanticKernelRequestDto
{
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public string? SessionId { get; set; }
    
    public bool IncludeHistory { get; set; } = true;
    
    /// <summary>
    /// Optional: Force weather plugin to be used
    /// If null, intent detection will be used automatically
    /// </summary>
    public string? PreferredPlugin { get; set; }
    
    /// <summary>
    /// Additional parameters for weather plugin
    /// </summary>
    public Dictionary<string, object>? PluginParameters { get; set; }
    
    /// <summary>
    /// Enable or disable automatic intent detection
    /// </summary>
    public bool AutoDetectIntent { get; set; } = true;
}

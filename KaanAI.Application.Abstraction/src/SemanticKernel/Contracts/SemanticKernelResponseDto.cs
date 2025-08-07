namespace KaanAI.Application.Abstraction.SemanticKernel.Contracts;

public class SemanticKernelResponseDto
{
    public string Response { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int TokensUsed { get; set; }
    
    /// <summary>
    /// The intent that was detected from the user message
    /// </summary>
    public string DetectedIntent { get; set; } = "general";
    
    /// <summary>
    /// The plugin that was used to process the request
    /// </summary>
    public string UsedPlugin { get; set; } = "GeneralChat";
    
    /// <summary>
    /// Confidence score of intent detection (0.0 - 1.0)
    /// </summary>
    public double IntentConfidence { get; set; } = 0.0;
    
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Time taken to process the request
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }
    
    /// <summary>
    /// Additional metadata from plugins
    /// </summary>
    public Dictionary<string, object>? PluginMetadata { get; set; }
}

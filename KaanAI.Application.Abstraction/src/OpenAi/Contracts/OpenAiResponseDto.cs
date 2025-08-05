namespace KaanAI.Application.Abstraction.OpenAi.Contracts;

public class OpenAiResponseDto
{
    public string Response { get; set; } = string.Empty;
    
    public string SessionId { get; set; } = string.Empty;
    
    public bool IsSuccess { get; set; } = true;
    
    public string? ErrorMessage { get; set; }
    
    public int TokensUsed { get; set; }
    
    public string? Model { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public TimeSpan? ResponseTime { get; set; }
    
    public string? FinishReason { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
}
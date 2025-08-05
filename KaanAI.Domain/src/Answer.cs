namespace KaanAI.Domain;
public class Answer
{
    public int Id { get; set; }

    public string AnswerText { get; set; } = string.Empty; //dönen answer tutulacak.

    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    public virtual ChatSession Session { get; set; } = null!;

    public int SessionId { get; set; }
    
    // Token usage tracking
    public int PromptTokens { get; set; } = 0;        // Input tokens used
    public int CompletionTokens { get; set; } = 0;    // Output tokens generated  
    public int TotalTokens { get; set; } = 0;         // Total tokens for this response
}
namespace KaanAI.Application.Abstraction.Chat.Contracts;

public class ChatMessageDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = string.Empty; // "Question" or "Answer"
} 
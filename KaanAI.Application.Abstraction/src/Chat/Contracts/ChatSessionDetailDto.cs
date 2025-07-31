namespace KaanAI.Application.Abstraction.Chat.Contracts;

public class ChatSessionDetailDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = new();
} 
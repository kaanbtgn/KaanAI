namespace KaanAI.Application.Abstraction.Chat.Contracts;

public class ChatSessionDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public int QuestionCount { get; set; }
    public int AnswerCount { get; set; }
} 
namespace KaanAI.Application.Abstraction.Chat.Contracts;

public class QuestionEntityDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime AskedAt { get; set; }
    public int SessionId { get; set; }
} 
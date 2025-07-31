namespace KaanAI.Application.Abstraction.Chat.Contracts;

public class AnswerEntityDto
{
    public int Id { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public DateTime AnsweredAt { get; set; }
    public int SessionId { get; set; }
} 
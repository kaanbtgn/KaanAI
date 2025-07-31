namespace KaanAI.Application.Abstraction.Chat.Contracts;

public class ChatSessionEntityDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public List<QuestionEntityDto> Questions { get; set; } = new();
    public List<AnswerEntityDto> Answers { get; set; } = new();
} 
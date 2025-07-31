namespace KaanAI.Application.Abstraction.Chat.Contracts;

public class AddQuestionRequest
{
    public int SessionId { get; set; }
    public string Content { get; set; } = string.Empty;
} 
namespace KaanAI.Domain.Entities;
public class Answer
{
    public int Id { get; set; }
    public string AnswerText { get; set; } //dönen answer tutulacak.
    public DateTime AnsweredAt { get; set; }
    public ChatSession Session { get; set; }
    public int SessionId { get; set; }
    
}
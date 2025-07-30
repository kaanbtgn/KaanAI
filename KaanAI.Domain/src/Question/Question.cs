namespace KaanAI.Domain.Entities;

public class Question
{
    public int Id { get; set; }
    public string Content { get; set; } // girilen promptu burada tutacağız
    public DateTime AskedAt { get; set; } = DateTime.UtcNow;
    public ChatSession Session { get; set; } //navigation property
    public int SessionId { get; set; }
    
}
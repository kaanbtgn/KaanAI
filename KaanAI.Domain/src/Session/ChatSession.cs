
namespace KaanAI.Domain.Entities;
public class ChatSession
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Question> Questions { get; set; }
    public ICollection<Answer> Answers { get; set; }

}
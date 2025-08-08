namespace KaanAI.Domain;

public class ErrorLog
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public int SessionId { get; set; }

    public  required string Message { get; set; }

    public required string StackTrace { get; set; } 

}
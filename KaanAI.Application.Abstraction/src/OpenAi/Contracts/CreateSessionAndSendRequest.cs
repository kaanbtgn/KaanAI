using System.ComponentModel.DataAnnotations;

namespace KaanAI.Application.Abstraction.OpenAi.Contracts;

public class CreateSessionAndSendRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Mesaj boş olamaz")]
    [MaxLength(4000, ErrorMessage = "Mesaj 4000 karakterden uzun olamaz")]
    public string Message { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Range(1, 4000, ErrorMessage = "MaxTokens 1 ile 4000 arasında olmalıdır")]
    public int? MaxTokens { get; set; } = 1000;
    
    [Range(0.0, 2.0, ErrorMessage = "Temperature 0.0 ile 2.0 arasında olmalıdır")]
    public float? Temperature { get; set; } = 0.7f;
    
    public string? SystemMessage { get; set; }
}
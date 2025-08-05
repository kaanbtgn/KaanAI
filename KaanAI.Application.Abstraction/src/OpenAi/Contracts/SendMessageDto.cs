using System.ComponentModel.DataAnnotations;

namespace KaanAI.Application.Abstraction.OpenAi.Contracts;

public class SendMessageDto
{
    [Required]
    [MinLength(1, ErrorMessage = "Mesaj boş olamaz")]
    [MaxLength(4000, ErrorMessage = "Mesaj 4000 karakterden uzun olamaz")]
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Opsiyonel session ID. Boş bırakılırsa otomatik olarak mevcut session kullanılır veya yeni session oluşturulur.
    /// </summary>
    public string? SessionId { get; set; }
    
    [Range(1, 4000, ErrorMessage = "MaxTokens 1 ile 4000 arasında olmalıdır")]
    public int? MaxTokens { get; set; } = 1000;
    
    [Range(0.0, 2.0, ErrorMessage = "Temperature 0.0 ile 2.0 arasında olmalıdır")]
    public float? Temperature { get; set; } = 0.7f;
    
    public string? SystemMessage { get; set; }
    
    /// <summary>
    /// True olduğunda session geçmişi dahil edilir. Varsayılan olarak true.
    /// </summary>
    public bool IncludeHistory { get; set; } = true;
}
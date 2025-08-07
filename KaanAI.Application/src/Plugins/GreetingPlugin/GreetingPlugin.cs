using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace KaanAI.Application.Plugins.GreetingPlugin;

/// <summary>
/// Greeting Plugin - Handles greetings and introductions to the AI assistant
/// Provides friendly and informative responses to user greetings
/// </summary>
public class GreetingPlugin
{
    /// <summary>
    /// Provides a friendly greeting and introduction to the AI assistant's capabilities
    /// </summary>
    /// <param name="message">The user's greeting message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A personalized greeting response</returns>
    [KernelFunction("GetGreeting")]
    [Description("Provides friendly greetings and introductions to the AI assistant's capabilities")]
    public async Task<string> GetGreetingAsync(
        [Description("The user's greeting message")] string message,
        CancellationToken cancellationToken = default)
    {
        // Determine greeting type based on message content
        var greetingType = DetermineGreetingType(message);
        
        return await Task.FromResult(GenerateGreetingResponse(greetingType));
    }

    /// <summary>
    /// Determines the type of greeting based on the user's message
    /// </summary>
    /// <param name="message">User's greeting message</param>
    /// <returns>Greeting type identifier</returns>
    private string DetermineGreetingType(string message)
    {
        var lowerMessage = message.ToLowerInvariant();
        
        if (lowerMessage.Contains("günaydın") || lowerMessage.Contains("good morning"))
            return "morning";
        if (lowerMessage.Contains("iyi akşam") || lowerMessage.Contains("good evening"))
            return "evening";
        if (lowerMessage.Contains("iyi gece") || lowerMessage.Contains("good night"))
            return "night";
        if (lowerMessage.Contains("nasılsın") || lowerMessage.Contains("how are you"))
            return "casual";
        
        return "general";
    }

    /// <summary>
    /// Generates an appropriate greeting response based on the greeting type
    /// </summary>
    /// <param name="greetingType">Type of greeting to generate</param>
    /// <returns>Personalized greeting message</returns>
    private string GenerateGreetingResponse(string greetingType)
    {
        return greetingType switch
        {
            "morning" => "🌅 Günaydın! Ben KaanAI, sizin yapay zeka asistanınızım. Yeni bir güne başlarken size hava durumu, borsa bilgileri ve OCR/metin tanıma konularında yardımcı olabilirim. Güzel bir gün geçirmeniz için hangi bilgilere ihtiyacınız var?",
            
            "evening" => "🌆 İyi akşamlar! Ben KaanAI asistanınızım. Akşam saatlerinde size hava durumu tahminleri, finansal veriler ve doküman işleme konularında destek sağlayabilirim. Size nasıl yardımcı olabilirim?",
            
            "night" => "🌙 İyi geceler! Ben KaanAI. Gece geç saatlerde de size hizmet vermeye devam ediyorum. Hava durumu, borsa ve OCR hizmetleri konularında yardımcı olabilirim. Ne öğrenmek istersiniz?",
            
            "casual" => "😊 Merhaba! Ben çok iyiyim, teşekkür ederim! Ben KaanAI, sizin AI asistanınızım. Hava durumu analizi, finansal veri takibi ve metin/doküman işleme konularında uzmanım. Siz nasılsınız? Size nasıl yardımcı olabilirim?",
            
            _ => "👋 Merhaba! Ben KaanAI asistanınızım. Size hava durumu, borsa bilgileri ve OCR/metin tanıma konularında yardımcı olabilirim. Bu konulardan herhangi biri hakkında soru sorabilir veya bilgi alabilirsiniz. Nasıl yardımcı olabilirim? 🤖"
        };
    }

    /// <summary>
    /// Gets a list of available assistant capabilities
    /// </summary>
    /// <returns>Description of assistant capabilities</returns>
    [KernelFunction("GetCapabilities")]
    [Description("Returns detailed information about the AI assistant's capabilities and available services")]
    public string GetCapabilities()
    {
        return @"🤖 **KaanAI Yetenekleri:**

🌤️ **Hava Durumu Servisleri:**
- Anlık hava durumu bilgileri
- 5 günlük hava tahminleri
- Detaylı meteoroloji analizi
- Giyim ve aktivite önerileri

📈 **Borsa & Finans:**
- Hisse senedi fiyat takibi
- Piyasa analizi
- Finansal veri raporlama

📄 **OCR & Metin İşleme:**
- Görüntülerden metin çıkarma
- Doküman analizi
- Metin tanıma ve işleme

Bu hizmetlerden herhangi birini kullanmak için sadece sorunuzu sorun, ben size yardımcı olayım! 🚀";
    }
}
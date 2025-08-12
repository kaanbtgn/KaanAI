using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace KaanAI.Application.Plugins;

/// <summary>
/// Greeting Plugin - Handles greetings and introductions to the AI assistant
/// Provides friendly and informative responses to user greetings
/// </summary>
public class GreetingPlugin
{
    private readonly Kernel _kernel;
    public GreetingPlugin(Kernel kernel)
    {
        _kernel = kernel;
    }

    /// <summary>
    /// Provides a friendly greeting and introduction to the AI assistant's capabilities
    /// </summary>
    /// <param name="message">The user's greeting message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A personalized greeting response</returns>
[KernelFunction("GetGreeting")]
    [Description("Kullanıcının mesajına uygun, zamanı dikkate alan kısa bir selamlama döner.")]
    public async Task<string> GetGreetingAsync(
        [Description("Kullanıcının selamlama mesajı")] string message,
        [Description("Kullanıcının yerel tarih/saat bilgisi")] DateTimeOffset? dateTime = null,
        CancellationToken cancellationToken = default)
    {
        message = message?.Trim() ?? string.Empty;

        // Parametre olarak verilen zamanı kullan; UtcNow'a gitme
        var localTime = dateTime ?? DateTimeOffset.Now;
        var isoTime = localTime.ToString("O"); // ISO 8601, LLM için stabil

        var prompt = $@"
Kullanıcının selamını kibarca karşıla, kendini kısaca tanıt.
Kurallar:
- Sadece selamı yanıtla; gereksiz uzatma.
- Mesajda ek içerik varsa kısaca dikkate al.
- Gönderim zamanına göre uygun selamlama kullan: sabah=günaydın, öğleden sonra=iyi günler, akşam=iyi akşamlar, gece=iyi geceler.
- Türkçe yanıt ver, tek paragraf, 1-2 cümle.
- KaanAI asistanı olarak kendini tanıt ve yapabileceklerin hakkında bilgi ver.
- Selamlama ve yapabileceklerin haricinde kullanıcının mesajını kibarca reddet ve yalnızca yapabileceklerin hakkında bilgi ver.

Kullanıcının Mesajı:
{message}

Gönderim Zamanı (ISO 8601):
{isoTime}

Yanıt:";
        var fn = _kernel.CreateFunctionFromPrompt(prompt);
        var result = await _kernel.InvokeAsync(fn, new KernelArguments(), cancellationToken);
        return result.GetValue<string>() ?? "Merhaba! Ben KaanAI. Size nasıl yardımcı olabilirim? Hava durumu, borsa ve metin işleme konularında yardımcı olabilirim.";
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
- Kripto para birimi fiyat takibi
- Piyasa analizi
- Finansal veri raporlama

📄 **OCR & Metin İşleme:**
- Görüntülerden metin çıkarma
- Doküman analizi
- Metin tanıma ve işleme
- Metin özetlemesi ve başlık çıkarma

Bu hizmetlerden herhangi birini kullanmak için sadece sorunuzu sorun, ben size yardımcı olayım! 🚀";
    }
}
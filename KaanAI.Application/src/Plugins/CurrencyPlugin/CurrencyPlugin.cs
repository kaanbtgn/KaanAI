using System.ComponentModel;
using System.Text;
using KaanAI.Application.Abstraction.Currency;
using Microsoft.SemanticKernel;

namespace KaanAI.Application.Plugins.CurrencyPlugin;

/// <summary>
/// Currency plugin exposes currency/ticker endpoints to the LLM via SK function calling
/// </summary>
public class CurrencyPlugin
{
    private readonly ICurrencyService _currencyService;
    private readonly Kernel _kernel;

    public CurrencyPlugin(ICurrencyService currencyService, Kernel kernel)
    {
        _currencyService = currencyService;
        _kernel = kernel;
    }

    [KernelFunction("get_ticker")]
    [Description("Get latest price and 24h stats for a currency/crypto pair (e.g., BTC/EUR, EUR/USD, USD/TRY). Also returns an AI commentary in Turkish.")]
    public async Task<string> GetTickerAsync(
        [Description("Pair or question like 'BTC/EUR', 'EUR/USD', 'USD/TRY', 'dolar ne kadar', 'bitcoin ne kadar'")] string message,
        CancellationToken cancellationToken = default)
    {
        // 1) Let LLM extract the pair/symbol
        var extractPrompt = @"Aşağıdaki mesajdan finansal çifti veya sembolü çıkar. 
Sadece sembol ya da çift döndür (ör. BTC/EUR, EUR/USD, USD/TRY, BTC/USD). 
Türkçe mesajları da anla. Örnekler: 
- 'dolar ne kadar' -> USD/TRY
- 'euro kaç para' -> EUR/TRY
- 'bitcoin ne kadar' -> BTC/USD
Mesaj: " + message + "\nCevap:";
        var fn = _kernel.CreateFunctionFromPrompt(extractPrompt);
        var extracted = (await _kernel.InvokeAsync(fn, cancellationToken: cancellationToken)).GetValue<string>()?.Trim() ?? message;

        var quote = await _currencyService.GetTickerAsync(extracted, cancellationToken);
        if (quote == null)
        {
            return $"'{extracted}' için fiyat bilgisi alınamadı.";
        }

        var facts = $"Pair: {quote.Pair}\nLast: {quote.LastPrice}\nAsk: {quote.Ask}\nBid: {quote.Bid}\nHigh24h: {quote.High24h}\nLow24h: {quote.Low24h}\nOpen: {quote.OpenToday}\nSource: {quote.Source}\nTimeUtc: {quote.RetrievedAtUtc:yyyy-MM-dd HH:mm:ss}";

        // 2) Ask LLM to comment in Turkish, including simple risk disclaimer
        var analysisPrompt = @"Kullanıcı finansal fiyat sordu. Aşağıdaki gerçekleri kullanarak Türkçe kısa bir yorum yaz:
- Fiyatı açık ve anlaşılır yaz
- Eğer kullanıcı 'yükselir mi' gibi bir görüş istediyse, temkinli, kısa ve genel bir değerlendirme yap
- Kesin yatırım tavsiyesi verme, küçük bir uyarı ekle
Gerçekler:\n" + facts + "\n\nCevap:";
        var analysisFn = _kernel.CreateFunctionFromPrompt(analysisPrompt);
        var comment = (await _kernel.InvokeAsync(analysisFn, cancellationToken: cancellationToken)).GetValue<string>() ?? string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine($"💱 {quote.Pair} Anlık Fiyat");
        sb.AppendLine($"Son: {quote.LastPrice} | Alış: {quote.Bid} | Satış: {quote.Ask}");
        if (quote.High24h > 0 || quote.Low24h > 0)
        {
            sb.AppendLine($"24s Yüksek: {quote.High24h} | 24s Düşük: {quote.Low24h}");
        }
        if (quote.OpenToday > 0)
        {
            sb.AppendLine($"Açılış: {quote.OpenToday}");
        }
        sb.AppendLine($"Kaynak: {quote.Source} | Zaman (UTC): {quote.RetrievedAtUtc:yyyy-MM-dd HH:mm:ss}");
        if (!string.IsNullOrWhiteSpace(comment))
        {
            sb.AppendLine();
            sb.AppendLine("🤖 Yorum:");
            sb.AppendLine(comment.Trim());
        }
        return sb.ToString();
    }

    [KernelFunction("get_ohlc")]
    [Description("Get OHLC candles for a pair and interval, then provide a short AI observation in Turkish")]
    public async Task<string> GetOhlcAsync(
        [Description("Symbol pair like 'BTC/EUR', 'EUR/USD'")] string pair,
        [Description("Interval in minutes (e.g., 1, 5, 15, 60, 240, 1440)")] string interval = "60",
        CancellationToken cancellationToken = default)
    {
        var candles = await _currencyService.GetOhlcAsync(pair, interval, null, cancellationToken);
        if (candles.Count == 0)
        {
            return $"'{pair}' çifti için OHLC verisi bulunamadı.";
        }

        var last = candles.TakeLast(20).ToList();
        var sb = new StringBuilder();
        sb.AppendLine($"📊 {pair} OHLC (son {last.Count} mum, interval {interval}m)");
        foreach (var c in last)
        {
            var dt = DateTimeOffset.FromUnixTimeSeconds(c.Timestamp).UtcDateTime;
            sb.AppendLine($"{dt:MM-dd HH:mm}  O:{c.Open} H:{c.High} L:{c.Low} C:{c.Close} V:{c.Volume}");
        }

        // Ask LLM for a quick pattern observation
        var ohlcText = sb.ToString();
        var obsPrompt = @"Aşağıdaki OHLC özetine bakarak Türkçe kısa bir gözlem yaz. 
- Trend var mı, volatilite nasıl? Yükseliyor mu, düşüyor mu? Önümüzdeki 1 saatte ne olması bekleniyor? Tehlike var mı?
- Yatırım tavsiyesi verme.
Metin:\n" + ohlcText + "\n\nCevap:";
        var obsFn = _kernel.CreateFunctionFromPrompt(obsPrompt);
        var obs = (await _kernel.InvokeAsync(obsFn, cancellationToken: cancellationToken)).GetValue<string>() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(obs))
        {
            sb.AppendLine();
            sb.AppendLine("🤖 Gözlem:");
            sb.AppendLine(obs.Trim());
        }
        return sb.ToString();
    }
}



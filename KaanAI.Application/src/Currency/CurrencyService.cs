using System.Text.Json;
using KaanAI.Application.Abstraction;
using KaanAI.Application.Abstraction.Currency;
using KaanAI.Application.Abstraction.Currency.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace KaanAI.Application.Currency;

/// <summary>
/// Service for fetching currency/crypto/forex market data using Twelve Data API
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TwelveDataSettings _twelveDataSettings;
    private readonly ILogger<CurrencyService> _logger;

    public CurrencyService(
        IHttpClientFactory httpClientFactory,
        IOptions<TwelveDataSettings> twelveDataSettings,
        ILogger<CurrencyService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _twelveDataSettings = twelveDataSettings.Value;
        _logger = logger;
    }

    public async Task<CurrencyQuoteDto?> GetTickerAsync(string pair, CancellationToken cancellationToken = default)
    {
        return await TryGetQuoteFromTwelveData(pair, cancellationToken);
    }

    public async Task<IReadOnlyList<OhlcCandleDto>> GetOhlcAsync(string pair, string interval = "1", long? since = null, CancellationToken cancellationToken = default)
    {
        return await TryGetOhlcFromTwelveData(pair, interval, since, cancellationToken);
    }

    private static decimal ParseDecimal(string? s)
    {
        return decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)
            ? d
            : 0m;
    }

    private async Task<CurrencyQuoteDto?> TryGetQuoteFromTwelveData(string pair, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_twelveDataSettings.ApiKey)) return null;
            var http = _httpClientFactory.CreateClient("TwelveData");
            var symbol = pair.Trim().ToUpperInvariant().Replace(" ", string.Empty);
            var url = $"quote?symbol={Uri.EscapeDataString(symbol)}&apikey={_twelveDataSettings.ApiKey}";
            _logger.LogInformation("Requesting TwelveData quote for symbol: {Symbol}", symbol);
            using var resp = await http.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("symbol", out _))
            {
                // could be error or a list; try 'data' array first element
                if (doc.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0)
                {
                    var first = data[0];
                    if (!first.TryGetProperty("close", out var closeEl)) return null;
                    var lastVal = closeEl.GetDecimal();
                    return new CurrencyQuoteDto
                    {
                        Pair = symbol,
                        LastPrice = lastVal,
                        RetrievedAtUtc = DateTime.UtcNow,
                        Source = "TwelveData"
                    };
                }
                return null;
            }
            // standard quote result
            var lastStr = doc.RootElement.GetProperty("close").GetString();
            var last = ParseDecimal(lastStr);
            var ask = ParseDecimal(doc.RootElement.TryGetProperty("ask", out var askEl) ? askEl.GetString() : null);
            var bid = ParseDecimal(doc.RootElement.TryGetProperty("bid", out var bidEl) ? bidEl.GetString() : null);
            var high = ParseDecimal(doc.RootElement.TryGetProperty("high", out var highEl) ? highEl.GetString() : null);
            var low = ParseDecimal(doc.RootElement.TryGetProperty("low", out var lowEl) ? lowEl.GetString() : null);
            return new CurrencyQuoteDto
            {
                Pair = symbol,
                LastPrice = last,
                Ask = ask,
                Bid = bid,
                High24h = high,
                Low24h = low,
                RetrievedAtUtc = DateTime.UtcNow,
                Source = "TwelveData"
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "TwelveData quote request failed.");
            return null;
        }
    }
    private async Task<IReadOnlyList<OhlcCandleDto>> TryGetOhlcFromTwelveData(string pair, string interval, long? since, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_twelveDataSettings.ApiKey)) return Array.Empty<OhlcCandleDto>();
            var http = _httpClientFactory.CreateClient("TwelveData");
            var symbol = pair.Trim().ToUpperInvariant().Replace(" ", string.Empty);
            // map minutes to TwelveData interval string
            var iv = interval switch
            {
                "1" => "1min",
                "5" => "5min",
                "15" => "15min",
                "60" => "1h",
                "240" => "4h",
                "1440" => "1day",
                _ => "1h"
            };
            var url = $"time_series?symbol={Uri.EscapeDataString(symbol)}&interval={iv}&outputsize=100&apikey={_twelveDataSettings.ApiKey}";
            _logger.LogInformation("Requesting TwelveData time_series for symbol: {Symbol}, interval: {Interval}", symbol, iv);
            using var resp = await http.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return Array.Empty<OhlcCandleDto>();
            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("values", out var values) || values.ValueKind != JsonValueKind.Array) return Array.Empty<OhlcCandleDto>();
            var list = new List<OhlcCandleDto>();
            foreach (var v in values.EnumerateArray())
            {
                if (!v.TryGetProperty("datetime", out var dtEl)) continue;
                var dt = DateTime.Parse(dtEl.GetString()!);
                var open = ParseDecimal(v.GetProperty("open").GetString());
                var high = ParseDecimal(v.GetProperty("high").GetString());
                var low = ParseDecimal(v.GetProperty("low").GetString());
                var close = ParseDecimal(v.GetProperty("close").GetString());
                var volume = ParseDecimal(v.TryGetProperty("volume", out var volEl) ? volEl.GetString() : null);
                list.Add(new OhlcCandleDto
                {
                    Timestamp = new DateTimeOffset(dt).ToUnixTimeSeconds(),
                    Open = open,
                    High = high,
                    Low = low,
                    Close = close,
                    Volume = volume,
                    Source = "TwelveData"
                });
            }
            // TwelveData returns newest first; ensure chronological order
            list.Reverse();
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "TwelveData OHLC request failed.");
            return Array.Empty<OhlcCandleDto>();
        }
    }
}



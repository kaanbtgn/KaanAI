namespace KaanAI.Application.Abstraction.Currency.Contracts;

public class CurrencyQuoteDto
{
    public string Pair { get; set; } = string.Empty; // e.g., BTCEUR, EURUSD, USDTRY
    public decimal LastPrice { get; set; }
    public decimal Ask { get; set; }
    public decimal Bid { get; set; }
    public decimal High24h { get; set; }
    public decimal Low24h { get; set; }
    public decimal OpenToday { get; set; }
    public DateTime RetrievedAtUtc { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = string.Empty; // Marketstack or Kraken
}



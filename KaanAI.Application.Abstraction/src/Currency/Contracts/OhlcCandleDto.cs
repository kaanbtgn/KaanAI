namespace KaanAI.Application.Abstraction.Currency.Contracts;

public class OhlcCandleDto
{
    public long Timestamp { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public string Source { get; set; } = string.Empty; // Marketstack or Kraken
}



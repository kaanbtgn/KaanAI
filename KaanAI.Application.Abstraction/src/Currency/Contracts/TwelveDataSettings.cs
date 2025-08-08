namespace KaanAI.Application.Abstraction.Currency.Contracts;

public class TwelveDataSettings
{
    public const string SectionName = "TwelveData";
    public string BaseUrl { get; set; } = "https://api.twelvedata.com";
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 15;
}



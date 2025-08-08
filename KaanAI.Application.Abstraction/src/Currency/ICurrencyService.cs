using KaanAI.Application.Abstraction.Currency.Contracts;

namespace KaanAI.Application.Abstraction.Currency;

public interface ICurrencyService : IService
{
    Task<CurrencyQuoteDto?> GetTickerAsync(string pair, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OhlcCandleDto>> GetOhlcAsync(string pair, string interval = "1", long? since = null, CancellationToken cancellationToken = default);
}



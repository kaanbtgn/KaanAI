using KaanAI.Application.Abstraction.OpenAI.Contracts;

namespace KaanAI.Application.Abstraction.OpenAI;

public interface IOpenAI : IService
{
    /// <summary>
    /// OpenAI'ya basit mesaj gönderir
    /// </summary>
    /// <param name="request">Gönderilecek mesaj bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>OpenAI'dan gelen yanıt</returns>
    Task<OpenAIResponseDto> SendMessageAsync(SendMessageDto request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// OpenAI'ya geçmiş konuşma bağlamı ile birlikte mesaj gönderir
    /// </summary>
    /// <param name="request">Gönderilecek mesaj bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>OpenAI'dan gelen yanıt</returns>
    Task<OpenAIResponseDto> SendMessageWithHistoryAsync(SendMessageDto request, CancellationToken cancellationToken = default);
}
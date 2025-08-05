using KaanAI.Application.Abstraction.OpenAi.Contracts;

namespace KaanAI.Application.Abstraction;

public interface IOpenAiService : IService
{
    /// <summary>
    /// OpenAI'ya mesaj gönderir ve otomatik session yönetimi yapar
    /// Session ID yoksa otomatik olarak yeni session oluşturur
    /// </summary>
    /// <param name="request">Gönderilecek mesaj bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>OpenAI'dan gelen yanıt ve session bilgisi</returns>
    Task<OpenAiResponseDto> SendMessageAsync(SendMessageDto request, CancellationToken cancellationToken = default);
}
using KaanAI.Application.Abstraction.Chat.Contracts;
namespace KaanAI.Application.Abstraction;

public interface IChatService : IService
{
    Task<ChatSessionDto> CreateSessionAsync(string? createdBy = null);
    Task<ChatSessionDto> GetOrCreateCurrentSessionAsync();
    Task<ChatSessionDetailDto?> GetSessionAsync(int sessionId);
    Task<IEnumerable<ChatSessionDto>> GetAllSessionsAsync();
    Task<ChatMessageDto> AddQuestionAsync(int sessionId, string content);
    Task<ChatMessageDto> AddAnswerAsync(int sessionId, string answerText);
    Task<ChatMessageDto> AddAnswerAsync(int sessionId, string answerText, int promptTokens, int completionTokens, int totalTokens);
    Task<IEnumerable<ChatMessageDto>> GetSessionQuestionsAsync(int sessionId);
    Task<IEnumerable<ChatMessageDto>> GetSessionAnswersAsync(int sessionId);
} 
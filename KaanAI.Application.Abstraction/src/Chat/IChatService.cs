using KaanAI.Application.Abstraction.Chat.Contracts;

namespace KaanAI.Application.Abstraction.Chat;

public interface IChatService
{
    Task<ChatSessionDto> CreateSessionAsync(string createdBy);
    Task<ChatSessionDetailDto?> GetSessionAsync(int sessionId);
    Task<IEnumerable<ChatSessionDto>> GetSessionsByUserAsync(string userId);
    Task<ChatMessageDto> AddQuestionAsync(int sessionId, string content);
    Task<ChatMessageDto> AddAnswerAsync(int sessionId, string answerText);
    Task<IEnumerable<ChatMessageDto>> GetSessionQuestionsAsync(int sessionId);
    Task<IEnumerable<ChatMessageDto>> GetSessionAnswersAsync(int sessionId);
} 
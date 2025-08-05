using KaanAI.Application.Abstraction;
using KaanAI.Application.Abstraction.Chat;
using KaanAI.Application.Abstraction.Chat.Contracts;
using KaanAI.Domain;

namespace KaanAI.Application;

public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;
    private const string DEFAULT_USER = "system_user";

    public ChatService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ChatSessionDto> CreateSessionAsync(string? createdBy = null)
    {
        var session = new ChatSession
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdBy ?? DEFAULT_USER,
            Questions = new List<Question>(),
            Answers = new List<Answer>()
        };

        await _unitOfWork.Repository<ChatSession>().AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return new ChatSessionDto
        {
            Id = session.Id,
            CreatedAt = session.CreatedAt,
            CreatedBy = session.CreatedBy,
            UpdatedAt = session.UpdatedAt,
            QuestionCount = 0,
            AnswerCount = 0
        };
    }

    public async Task<ChatSessionDetailDto?> GetSessionAsync(int sessionId)
    {
        var session = await _unitOfWork.Repository<ChatSession>().GetByIdAsync(sessionId);
        if (session == null)
            return null;

        var questions = await GetSessionQuestionsAsync(sessionId);
        var answers = await GetSessionAnswersAsync(sessionId);

        var messages = new List<ChatMessageDto>();
        
        // Add questions
        foreach (var question in questions.OrderBy(q => q.Timestamp))
        {
            messages.Add(question);
        }

        // Add answers
        foreach (var answer in answers.OrderBy(a => a.Timestamp))
        {
            messages.Add(answer);
        }

        // Sort all messages by timestamp
        messages = messages.OrderBy(m => m.Timestamp).ToList();

        return new ChatSessionDetailDto
        {
            Id = session.Id,
            CreatedAt = session.CreatedAt,
            CreatedBy = session.CreatedBy,
            UpdatedAt = session.UpdatedAt,
            Messages = messages
        };
    }

    public async Task<ChatSessionDto> GetOrCreateCurrentSessionAsync()
    {
        // For single user system, always get the most recent session or create new one
        var sessions = await _unitOfWork.Repository<ChatSession>().FindAsync(s => s.CreatedBy == DEFAULT_USER);
        var latestSession = sessions.OrderByDescending(s => s.UpdatedAt).FirstOrDefault();
        
        if (latestSession != null)
        {
            return new ChatSessionDto
            {
                Id = latestSession.Id,
                CreatedAt = latestSession.CreatedAt,
                CreatedBy = latestSession.CreatedBy,
                UpdatedAt = latestSession.UpdatedAt,
                QuestionCount = latestSession.Questions?.Count ?? 0,
                AnswerCount = latestSession.Answers?.Count ?? 0
            };
        }
        
        // No session exists, create a new one
        return await CreateSessionAsync();
    }

    public async Task<IEnumerable<ChatSessionDto>> GetAllSessionsAsync()
    {
        // Get sessions with their questions and answers count directly from database
        var sessions = await _unitOfWork.Repository<ChatSession>().GetAllAsync();
        var result = new List<ChatSessionDto>();
        
        foreach (var session in sessions.Where(s => s.CreatedBy == DEFAULT_USER))
        {
            // Get actual counts from database
            var questions = await _unitOfWork.Repository<Question>().FindAsync(q => q.SessionId == session.Id);
            var answers = await _unitOfWork.Repository<Answer>().FindAsync(a => a.SessionId == session.Id);
            
            result.Add(new ChatSessionDto
            {
                Id = session.Id,
                CreatedAt = session.CreatedAt,
                CreatedBy = session.CreatedBy,
                UpdatedAt = session.UpdatedAt,
                QuestionCount = questions.Count(),
                AnswerCount = answers.Count()
            });
        }
        
        return result.OrderByDescending(s => s.UpdatedAt);
    }

    public async Task<ChatMessageDto> AddQuestionAsync(int sessionId, string content)
    {
        var session = await _unitOfWork.Repository<ChatSession>().GetByIdAsync(sessionId);
        if (session == null)
            throw new ArgumentException($"Session with ID {sessionId} not found");

        var question = new Question
        {
            Content = content,
            AskedAt = DateTime.UtcNow,
            SessionId = sessionId
        };

        await _unitOfWork.Repository<Question>().AddAsync(question);
        
        // Update session timestamp
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<ChatSession>().UpdateAsync(session);
        
        await _unitOfWork.SaveChangesAsync();

        return new ChatMessageDto
        {
            Id = question.Id,
            Content = question.Content,
            Timestamp = question.AskedAt,
            Type = "Question"
        };
    }

    public async Task<ChatMessageDto> AddAnswerAsync(int sessionId, string answerText)
    {
        var session = await _unitOfWork.Repository<ChatSession>().GetByIdAsync(sessionId);
        if (session == null)
            throw new ArgumentException($"Session with ID {sessionId} not found");

        var answer = new Answer
        {
            AnswerText = answerText,
            AnsweredAt = DateTime.UtcNow,
            SessionId = sessionId
        };

        await _unitOfWork.Repository<Answer>().AddAsync(answer);
        
        // Update session timestamp
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<ChatSession>().UpdateAsync(session);
        
        await _unitOfWork.SaveChangesAsync();

        return new ChatMessageDto
        {
            Id = answer.Id,
            Content = answer.AnswerText,
            Timestamp = answer.AnsweredAt,
            Type = "Answer"
        };
    }

    public async Task<IEnumerable<ChatMessageDto>> GetSessionQuestionsAsync(int sessionId)
    {
        var questions = await _unitOfWork.Repository<Question>().FindAsync(q => q.SessionId == sessionId);
        
        return questions.Select(q => new ChatMessageDto
        {
            Id = q.Id,
            Content = q.Content,
            Timestamp = q.AskedAt,
            Type = "Question"
        });
    }

    public async Task<IEnumerable<ChatMessageDto>> GetSessionAnswersAsync(int sessionId)
    {
        var answers = await _unitOfWork.Repository<Answer>().FindAsync(a => a.SessionId == sessionId);
        
        return answers.Select(a => new ChatMessageDto
        {
            Id = a.Id,
            Content = a.AnswerText,
            Timestamp = a.AnsweredAt,
            Type = "Answer"
        });
    }
} 
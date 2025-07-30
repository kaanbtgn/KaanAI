using KaanAI.Domain.Entities;
namespace KaanAI.Domain.Repositories;

public interface IUnitOfWork
{
    IGenericRepository<ChatSession>  ChatSessions { get; }
    IGenericRepository<Question>  Questions { get; }
    IGenericRepository<ErrorLog>  ErrorLogs { get; }
    IGenericRepository<Answer>  Answers { get; }
    Task<int> SaveAsync();
}
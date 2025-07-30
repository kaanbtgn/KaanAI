using KaanAI.Domain.Entities;
using KaanAI.Persistance.Context.Main;
using KaanAI.Domain.Repositories;
namespace KaanAI.Persistance.Data;

public sealed class UnitOfWork :IUnitOfWork
{
    private readonly MainDbContext  _context;

    public UnitOfWork(MainDbContext context)
    {
        _context = context;
        ChatSessions=new GenericRepository<ChatSession>(_context);
        Questions=new GenericRepository<Question>(_context);
        ErrorLogs=new GenericRepository<ErrorLog>(_context);
        Answers=new GenericRepository<Answer>(_context);
    }
    public IGenericRepository<ChatSession> ChatSessions { get; }
    public IGenericRepository<Question> Questions { get; }
    public IGenericRepository<ErrorLog> ErrorLogs { get; }
    public IGenericRepository<Answer> Answers { get; }
    public Task<int> SaveAsync() =>  _context.SaveChangesAsync();
    public ValueTask DisposeAsync() =>  _context.DisposeAsync();

}
using KaanAI.Domain.Entities;
namespace KaanAI.Persistance.Context.Main;
using Microsoft.EntityFrameworkCore;

public class MainDbContext : DbContext
{
    public MainDbContext(DbContextOptions<MainDbContext> options) : base(options){}
    public DbSet<ChatSession>ChatSessions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<ErrorLog> ErrorLogs { get; set; }
    public DbSet<Question> Questions { get; set; }

}
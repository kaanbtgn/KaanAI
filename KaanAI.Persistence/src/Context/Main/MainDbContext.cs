namespace KaanAI.Persistence.Context.Main;

using KaanAI.Domain;
using Microsoft.EntityFrameworkCore;

public class MainDbContext : DbContext
{
    public MainDbContext(DbContextOptions<MainDbContext> options) : base(options){}
    
    public DbSet<ChatSession> ChatSessions { get; set; } = null!;
    public DbSet<Answer> Answers { get; set; } = null!;
    public DbSet<ErrorLog> ErrorLogs { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ChatSession configuration
        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        // Question configuration
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.AskedAt).IsRequired();
            entity.HasOne(e => e.Session)
                  .WithMany(s => s.Questions)
                  .HasForeignKey(e => e.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Answer configuration
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AnswerText).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.AnsweredAt).IsRequired();
            entity.HasOne(e => e.Session)
                  .WithMany(s => s.Answers)
                  .HasForeignKey(e => e.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ErrorLog configuration
        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.StackTrace).HasMaxLength(8000);
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}
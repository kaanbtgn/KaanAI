using KaanAI.Application.Services;
using KaanAI.Application.Services.CreateSession;
using KaanAI.Application.Services.SessionHistory;
using KaanAI.Application.Services.UpdateSession;
using KaanAI.Domain.Entities;
using KaanAI.Domain.Repositories;
using KaanAI.Persistance.Context.Main;
using KaanAI.Persistance.Data;
using Microsoft.EntityFrameworkCore;

namespace KaanAI.API;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDbContext<MainDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
        builder.Services.AddScoped<CreateSessionService>();
        builder.Services.AddScoped<UpdateSessionService>();
        builder.Services.AddScoped<SessionHistoryService>();

// MVC - Swagger

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
using KaanAI.Application.Abstraction;
using KaanAI.Application.Extensions;
using KaanAI.Persistence;
using KaanAI.Persistence.Context.Main;
using Microsoft.EntityFrameworkCore;

namespace KaanAI.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        
        // Add CORS for development
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DevelopmentPolicy",
                policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
        });
        
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        // Configure HttpClient with better timeout settings
        builder.Services.AddHttpClient("AzureOpenAI", client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5); // 5 minutes timeout
        });
        
        // Database Context
        builder.Services
            .AddDbContext<MainDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Register Services
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddApplicationServices();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors("DevelopmentPolicy");
        }

        // Skip HTTPS redirection for development to avoid warnings
        // app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
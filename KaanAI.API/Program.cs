using KaanAI.Application.Abstraction;
using KaanAI.Application.Extensions;
using KaanAI.Application;
using KaanAI.Persistence;
using KaanAI.Persistence.Context.Main;
using Microsoft.EntityFrameworkCore;
using KaanAI.Application.Abstraction.OpenWeatherMap.Contracts;

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
        
        // Configure OpenWeatherMap settings
        builder.Services.Configure<OpenWeatherMapSettings>(
            builder.Configuration.GetSection(OpenWeatherMapSettings.SectionName));
        
        // Register OpenWeatherMap HttpClient and Service
        builder.Services.AddHttpClient<IOpenWeatherMapService, OpenWeatherMapService>(client =>
        {
            var baseUrl = builder.Configuration["OpenWeatherMap:BaseUrl"] ?? "https://api.openweathermap.org";
            client.BaseAddress = new Uri($"{baseUrl}/data/2.5/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        // Database Context
        builder.Services
            .AddDbContext<MainDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Register Services
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddApplicationServices();
        
        // Register Semantic Kernel Services
        builder.Services.AddSemanticKernel(builder.Configuration);

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
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using KaanAI.Persistance.Context.Main;

public class MainDbContextFactory : IDesignTimeDbContextFactory<MainDbContext>
{
    public MainDbContext CreateDbContext(string[] args)
    {
        // Persistance klasöründen çalışıyorsan Presentation yoluna çık
        var basePath = Directory.GetCurrentDirectory();
        var appsettingsHere = Path.Combine(basePath, "appsettings.json");
        if (!File.Exists(appsettingsHere))
            basePath = Path.GetFullPath(Path.Combine(basePath, "..", "..", "Presentation", "KaanAI.API"));

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var cs = config.GetConnectionString("DefaultConnection");

        var options = new DbContextOptionsBuilder<MainDbContext>()
            .UseSqlServer(cs)
            .Options;

        return new MainDbContext(options);
    }
}
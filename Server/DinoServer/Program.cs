using DinoServer.Interfaces;
using DinoServer.Services;
using DinoServer.Users;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Telegram.Bot;
using Telegram.Bot.Types; // Добавьте этот using
using System.Diagnostics.CodeAnalysis;

namespace DinoServer;
[ExcludeFromCodeCoverage]
class Program
{
    
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Start");

        var builder = WebApplication.CreateBuilder(args);
        string con =
            $"server={Environment.GetEnvironmentVariable("DB_SERVER")};user={Environment.GetEnvironmentVariable("DB_USER")};password={Environment.GetEnvironmentVariable("DB_PASSWORD")};database={Environment.GetEnvironmentVariable("DB_NAME")};"; 
        //dino-db-service.default.svc.cluster.local    
        //"server=localhost;user=root;password=password;database=DinoDB;";
        var version = new MySqlServerVersion(new Version(8, 0, 11));
        builder.Services.AddDbContextFactory<UserContext>(options => options.UseMySql(con, version));
        
        builder.Services.AddScoped<IGetUsersService, GetUsersService>();
        builder.Services.AddScoped<IAddUserService, AddUserService>();
        builder.Services.AddMetricServer(options =>
        {
           options.Port = 5000;  // Порт для /metrics
        });
        
        builder.Services.AddControllers();
        
        
        

        var app = builder.Build();
        app.UseDeveloperExceptionPage();
        // Метрики Prometheus должны быть ДО UseRouting()
        app.UseMetricServer(url: "/metrics");  // Эндпоинт для сбора метрик
        app.UseHttpMetrics();  // Метрики HTTP-запросов

        app.UseStaticFiles();
        app.UseDefaultFiles();
        app.UseRouting();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        

        app.MapControllers();
        
        // ----------------- Telegram Bot -----------------
        var contextFactory = app.Services.GetRequiredService<IDbContextFactory<UserContext>>();
        TelegramService.Initialize(contextFactory);
        await TelegramService.SendMessage("Hi. Server is working");
        
        app.Run();
        Console.WriteLine("Server is working");
    }
    
}

    
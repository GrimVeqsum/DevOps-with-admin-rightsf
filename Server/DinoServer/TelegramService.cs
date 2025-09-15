using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Collections.Concurrent;
using System.Text;
using DinoServer.Users;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Polling;
using System.Diagnostics.CodeAnalysis;

namespace DinoServer;

[ExcludeFromCodeCoverage]
public static class TelegramService
{
    private static TelegramBotClient? _bot;
    private static readonly ConcurrentDictionary<long, bool> _chatIds = new();
    private static CancellationTokenSource? _cts;
    private static IDbContextFactory<UserContext>? _contextFactory;

    [ExcludeFromCodeCoverage]
    public static void Initialize(IDbContextFactory<UserContext> contextFactory, string token)
    {
        _contextFactory = contextFactory;
        if (_bot != null) return;

        _bot = new TelegramBotClient(token);
        _cts = new CancellationTokenSource();

        _bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
            cancellationToken: _cts.Token);
    }

    [ExcludeFromCodeCoverage]
    private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        if (update.Type == UpdateType.Message && update.Message != null)
        {
            Console.WriteLine($"Получено сообщение: {update.Message.Text}");
            var chatId = update.Message.Chat.Id;
            _chatIds.TryAdd(chatId, true);

            var text = update.Message.Text?.Trim();

            if (text == "/top")
            {
                try
                {
                    var leaderboard = await BuildLeaderboardAsync();
                    await bot.SendMessage(chatId, leaderboard);
                }
                catch (Exception ex)
                {
                    await bot.SendMessage(chatId, $"Ошибка при формировании таблицы лидеров: {ex.Message}");
                }
            }
        }
    }

    [ExcludeFromCodeCoverage]
    private static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"Telegram error: {exception.Message}");
        return Task.CompletedTask;
    }

    [ExcludeFromCodeCoverage]
    public static async Task SendMessage(string message)
    {
        if (_bot == null)
        {
            Console.WriteLine("TelegramService not initialized!");
            return;
        }

        foreach (var chatId in _chatIds.Keys)
        {
            try
            {
                await _bot.SendMessage(chatId, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send to {chatId}: {ex.Message}");
            }
        }
    }

    public static void Stop() => _cts?.Cancel();

    private static async Task<string> BuildLeaderboardAsync()
    {
        await using var db = _contextFactory!.CreateDbContext();
        var users = await db.Users
            .OrderByDescending(u => u.Score)
            .Take(10)
            .ToListAsync();

        if (!users.Any()) return "Лидеры пока отсутствуют.";

        var sb = new StringBuilder();
        sb.AppendLine("🏆 Таблица лидеров:");
        sb.AppendLine("Имя | Счёт");
        sb.AppendLine("-------------");
        foreach (var u in users)
        {
            sb.AppendLine($"{u.Name} | {u.Score}");
        }

        return sb.ToString();
    }
}

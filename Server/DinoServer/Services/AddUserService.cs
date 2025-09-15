using DinoServer.Interfaces;
using DinoServer.Users;
using Microsoft.EntityFrameworkCore;

namespace DinoServer.Services;

public class AddUserService : IAddUserService
{
    private readonly IDbContextFactory<UserContext> _contextFactory;

    public AddUserService(IDbContextFactory<UserContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<User> AddUserAsync(User user, int userId)
    {
        if (user == null)
        {
            throw new ArgumentException("Book data is invalid.");
        }
        await using var db = _contextFactory.CreateDbContext();
        await db.AddUserAsync(user);
        await TelegramService.SendMessage("Score added " + userId  +" for " + user.Name);
        return user;
    }
}
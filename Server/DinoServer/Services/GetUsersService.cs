using DinoServer.Users;
using DinoServer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DinoServer.Services;

public class GetUsersService : IGetUsersService
{
    private readonly IDbContextFactory<UserContext> _contextFactory;

    public GetUsersService(IDbContextFactory<UserContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        await using var db = _contextFactory.CreateDbContext();
        
        return await db.Users.ToListAsync();
    }
}
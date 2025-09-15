using DinoServer.Users;

namespace DinoServer.Interfaces;

public interface IGetUsersService
{
    Task<IEnumerable<User>> GetUsersAsync();
}
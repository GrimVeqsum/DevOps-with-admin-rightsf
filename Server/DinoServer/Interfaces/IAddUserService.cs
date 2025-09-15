using DinoServer.Users;

namespace DinoServer.Interfaces;

public interface IAddUserService
{
    Task<User> AddUserAsync(User user, int userId);
}
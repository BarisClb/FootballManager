using FootballManager.Application.Models.Requests;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Repositories.Dapper
{
    public interface IUserRepository : IBaseRepository
    {
        Task<User> CreateUser(CreateUserRequest createUserRequest);
        Task<User> GetUserById(int userId);
        Task<User> GetFirstUser();
        Task<User> GetUserByUsernameAndPassword(string username, string password);
        Task<IEnumerable<User>> GetManagersByMatchId(int matchId);
    }
}

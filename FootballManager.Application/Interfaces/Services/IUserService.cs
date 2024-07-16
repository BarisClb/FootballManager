using FootballManager.Application.Models.Requests;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<User> CreateUser(CreateUserRequest createUserRequest);
    }
}

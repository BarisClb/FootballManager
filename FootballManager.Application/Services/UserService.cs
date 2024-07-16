using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Models.Requests;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }


        public async Task<User> CreateUser(CreateUserRequest createUserRequest)
        {
            return await _userRepository.CreateUser(createUserRequest);
        }
    }
}

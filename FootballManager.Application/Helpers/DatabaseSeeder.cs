using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Models.Requests;
using FootballManager.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FootballManager.Application.Helpers
{
    public class DatabaseSeeder
    {
        private readonly IUserRepository _userRepository;
        private readonly IOptions<AuthSettings> _authSettings;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(IUserRepository userRepository, IOptions<AuthSettings> authSettings, ILogger<DatabaseSeeder> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _authSettings = authSettings ?? throw new ArgumentNullException(nameof(authSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async void SeedUserData()
        {
            var admin = await _userRepository.GetFirstUser();

            if (admin == null)
            {
                CreateUserRequest newUser = new() { Username = _authSettings.Value.UserUsername, Name = _authSettings.Value.UserName, Password = _authSettings.Value.AuthPassword };
                try
                {
                    admin = await _userRepository.CreateUser(newUser);
                    if (admin == null)
                        throw new Exception("Failed to CreateUser.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DatabaseSeeder.SeedData.CreateFirstUser Error: Failed to Create User. CreateUserRequest: '{JsonConvert.SerializeObject(newUser)}'. ErrorMessage: '{ex.Message}'");
                }
            }
        }
    }
}

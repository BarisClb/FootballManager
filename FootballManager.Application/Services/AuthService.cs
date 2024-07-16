using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Settings;
using Microsoft.Extensions.Options;

namespace FootballManager.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IOptions<AuthSettings> _authSettings;

        public AuthService(IOptions<AuthSettings> authSettings)
        {
            _authSettings = authSettings ?? throw new ArgumentNullException(nameof(authSettings));
        }


        public async Task<bool> VerifyAdminAccess(string username, string password)
        {
            return username == _authSettings.Value.AuthUsername && password == _authSettings.Value.AuthPassword;
        }
    }
}

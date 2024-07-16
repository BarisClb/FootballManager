namespace FootballManager.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<bool> VerifyAdminAccess(string username, string password);
    }
}

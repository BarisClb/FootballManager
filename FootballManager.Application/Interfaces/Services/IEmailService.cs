namespace FootballManager.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendMatchResult(string result, DateTime matchPlayedTime, string email);
    }
}

using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace FootballManager.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly IOptions<SeasonsSettings> _seasonsSettings;
        private readonly IOptions<ProjectSettings> _projectSettings;

        public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings, IOptions<SeasonsSettings> seasonsSettings, IOptions<ProjectSettings> projectSettings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
            _seasonsSettings = seasonsSettings ?? throw new ArgumentNullException(nameof(seasonsSettings));
            _projectSettings = projectSettings ?? throw new ArgumentNullException(nameof(projectSettings));
        }


        public async Task SendMatchResult(string result, DateTime matchPlayedTime, string email)
        {
            string subject = $"Match Result from {matchPlayedTime.AddHours(_projectSettings.Value.TimezoneDifference ?? 0).ToString("dd/MM/yyyy HH:mm")}";
            string text = $"{result}";

            try
            {
                await sendEmail(email, subject, text);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send Email to: '{email}', for the Match: '{result}'. ErrorMessage: {ex.ToString()}");
            }
        }


        private async Task sendEmail(string receiverEmail, string subject, string text)
        {
            if (string.IsNullOrEmpty(_emailSettings.Value.SenderEmail) || string.IsNullOrEmpty(_emailSettings.Value.SenderPassword))
                return;

            MailMessage mail = new()
            {
                From = new MailAddress(_emailSettings.Value.SenderEmail ?? ""),
                Subject = subject,
                Body = text,
                IsBodyHtml = true
            };
            mail.To.Add(receiverEmail);

            SmtpClient smtp = new()
            {
                Credentials = new NetworkCredential(_emailSettings.Value.SenderEmail ?? "", _emailSettings.Value.SenderPassword ?? ""),
                Port = 587,
                Host = "smtp.gmail.com",
                EnableSsl = true
            };

            smtp.Send(mail);
        }
    }
}

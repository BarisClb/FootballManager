using FootballManager.Application.Interfaces.Services;
using FootballManager.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FootballManager.Infrastructure
{
    public static class ServiceRegistration
    {
        public static void InfrastructureServiceRegistration(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, EmailService>();
        }
    }
}

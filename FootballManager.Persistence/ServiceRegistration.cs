using FootballManager.Application.Helpers;
using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Interfaces.Repositories.EFC;
using FootballManager.Persistence.Contexts;
using FootballManager.Persistence.Repositories.Dapper;
using FootballManager.Persistence.Repositories.EFC;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FootballManager.Persistence
{
    public static class ServiceRegistration
    {
        public static void PersistenceServiceRegistration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<FootballManagerDbContext>(options => options.UseSqlServer(configuration["MsSqlSettings:ConnectionString"]));

            services.AddScoped<IBaseRepository, BaseRepository>();
            services.AddScoped<IGoalRepository, GoalRepository>();
            services.AddScoped<IMatchRepository, MatchRepository>();
            services.AddScoped<IPlayerRepository, PlayerRepository>();
            services.AddScoped<ISeasonRepository, SeasonRepository>();
            services.AddScoped<ISeasonPassRepository, SeasonPassRepository>();
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IMatchEfcRepository, MatchEfcRepository>();
            services.AddScoped<IPlayerEfcRepository, PlayerEfcRepository>();
            services.AddScoped<ITeamEfcRepository, TeamEfcRepository>();
            services.AddScoped<ISeasonSettingsRepository, SeasonSettingsRepository>();
        }

        public static void InitializeMsSqlDb(this WebApplication app)
        {
            using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<FootballManagerDbContext>();
                RelationalDatabaseCreator databaseCreator = (RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>();
                context.Database.EnsureCreated();

                var databaseSeeder = serviceScope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

                if (!context.Users.Any())
                    databaseSeeder.SeedUserData();
            }
        }
    }
}

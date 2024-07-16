using FootballManager.Application.Helpers;
using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Services;
using FootballManager.Application.Settings;
using Hangfire;
using Hangfire.SqlServer;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FootballManager.Application
{
    public static class ServiceRegistration
    {
        public static void ApplicationServiceRegistration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AuthSettings>(configuration.GetSection("AuthSettings"));
            services.Configure<ElasticsearchSettings>(configuration.GetSection("ElasticsearchSettings"));
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.Configure<HangfireSettings>(configuration.GetSection("HangfireSettings"));
            services.Configure<MsSqlSettings>(configuration.GetSection("MsSqlSettings"));
            services.Configure<ProjectSettings>(configuration.GetSection("ProjectSettings"));
            services.Configure<ScheduleWeeklyLeagueSettings>(configuration.GetSection("ScheduleWeeklyLeagueSettings"));
            services.Configure<ScheduleWeeklyTournamentSettings>(configuration.GetSection("ScheduleWeeklyTournamentSettings"));
            services.Configure<SeasonsSettings>(configuration.GetSection("SeasonsSettings"));

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IGoalService, GoalService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IMatchService, MatchService>();
            services.AddScoped<IPlayerService, PlayerService>();
            services.AddScoped<ISeasonService, SeasonService>();
            services.AddScoped<ISeasonPassService, SeasonPassService>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<DatabaseSeeder>();

            DapperPlusSettings.RegisterBulkOptions();
        }

        public static void HangfireServiceRegistration(this IServiceCollection services, IConfiguration configuration)
        {
            string? hangfireConnectionString = configuration["HangfireSettings:ConnectionString"];
            services.AddHangfire(configuration => configuration.UseSimpleAssemblyNameTypeSerializer()
                                                                           .UseRecommendedSerializerSettings()
                                                                           .UseSqlServerStorage(hangfireConnectionString, new SqlServerStorageOptions
                                                                           {
                                                                               CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                                                                               SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                                                                               QueuePollInterval = TimeSpan.Zero,
                                                                               UseRecommendedIsolationLevel = true,
                                                                               DisableGlobalLocks = true,
                                                                               SchemaName = "FootballManagerProjectJobs",
                                                                           }));
            services.AddHangfireServer();
        }

        public static void HangfireDashboardRegistration(this WebApplication app, WebApplicationBuilder builder)
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] {
                        new HangfireCustomBasicAuthenticationFilter()
                        {
                            User = builder.Configuration.GetSection("HangfireSettings:Username").Value,
                            Pass = builder.Configuration.GetSection("HangfireSettings:Password").Value
                        }
                }
            });
        }

        public static void HangfireJobRegistration(this IServiceCollection services, IConfiguration configuration)
        {
            Hangfire.RecurringJob.AddOrUpdate<IJobService>(job => job.SimulateMatchesJob(), configuration["HangfireSettings:SimulateMatchesJobCronTime"]);
        }
    }
}

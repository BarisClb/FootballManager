namespace FootballManager.Application.Interfaces.Services
{
    public interface IJobService
    {
        Task SimulateMatchesJob();
        Task ConcludeSeasons();
        Task ConcludeLeagues();
        Task ProceedTournaments();
        Task ScheduleWeeklyLeague();
        Task ScheduleWeeklyTournament();
    }
}

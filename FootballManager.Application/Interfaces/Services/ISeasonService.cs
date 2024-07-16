using FootballManager.Application.Models.Requests;
using FootballManager.Application.Models.Responses;
using FootballManager.Application.Models.View;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Services
{
    public interface ISeasonService
    {
        Task<Season> CreateSeason(CreateSeasonRequest createSeasonRequest);
        Task<BaseResponse<bool>> ScheduleSeason(ScheduleSeasonRequest scheduleSeasonRequest);
        Task<BaseResponse<bool>> ScheduleLeagueSeason(ScheduleSeasonRequest scheduleSeasonRequest);
        Task<bool> CancelSeasonSchedule(int seasonId);
        Task<List<SeasonTeamStandings>> GetSeasonTeamStandings(int? seasonId);
        Task<List<SeasonPlayerStandings>> GetSeasonPlayerStandings(int? seasonId);
        Task<List<SeasonMatchFixtureGroup>> GetSeasonMatchFixture(int? seasonId);
        Task<BaseResponse<string>> ConcludeLeague(int seasonId);
        Task<BaseResponse<bool>> ScheduleTournamentSeason(ScheduleSeasonRequest scheduleSeasonRequest);
        Task<BaseResponse<string>> ProceedTournament(int seasonId);
        Task<BaseResponse<string>> ScheduleNextRoundOfTournament(int seasonId);
        Task<BaseResponse<string>> ConcludeTournament(int seasonId);
    }
}

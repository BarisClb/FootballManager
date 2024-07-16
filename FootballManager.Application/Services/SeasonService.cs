using FootballManager.Application.Models.Requests;
using FootballManager.Application.Models.Responses;
using FootballManager.Application.Models.View;
using FootballManager.Domain.Entities;
using FootballManager.Application.Interfaces.Services;

namespace FootballManager.Application.Services;

// The original implementation is withheld from this public historical source.
public sealed class SeasonService : ISeasonService
{
    public Task<Season> CreateSeason(CreateSeasonRequest createSeasonRequest) => throw new NotImplementedException("Enter your season logic here.");
    public Task<BaseResponse<bool>> ScheduleSeason(ScheduleSeasonRequest scheduleSeasonRequest) => throw new NotImplementedException("Enter your season logic here.");
    public Task<BaseResponse<bool>> ScheduleLeagueSeason(ScheduleSeasonRequest scheduleSeasonRequest) => throw new NotImplementedException("Enter your season logic here.");
    public Task<bool> CancelSeasonSchedule(int seasonId) => throw new NotImplementedException("Enter your season logic here.");
    public Task<List<SeasonTeamStandings>> GetSeasonTeamStandings(int? seasonId) => throw new NotImplementedException("Enter your season logic here.");
    public Task<List<SeasonPlayerStandings>> GetSeasonPlayerStandings(int? seasonId) => throw new NotImplementedException("Enter your season logic here.");
    public Task<List<SeasonMatchFixtureGroup>> GetSeasonMatchFixture(int? seasonId) => throw new NotImplementedException("Enter your season logic here.");
    public Task<BaseResponse<string>> ConcludeLeague(int seasonId) => throw new NotImplementedException("Enter your season logic here.");
    public Task<BaseResponse<bool>> ScheduleTournamentSeason(ScheduleSeasonRequest scheduleSeasonRequest) => throw new NotImplementedException("Enter your season logic here.");
    public Task<BaseResponse<string>> ProceedTournament(int seasonId) => throw new NotImplementedException("Enter your season logic here.");
    public Task<BaseResponse<string>> ScheduleNextRoundOfTournament(int seasonId) => throw new NotImplementedException("Enter your season logic here.");
    public Task<BaseResponse<string>> ConcludeTournament(int seasonId) => throw new NotImplementedException("Enter your season logic here.");
}

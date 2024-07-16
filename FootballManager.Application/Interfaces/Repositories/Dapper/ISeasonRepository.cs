using FootballManager.Application.Models.Dtos;
using FootballManager.Application.Models.Requests;
using FootballManager.Application.Models.View;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Repositories.Dapper
{
    public interface ISeasonRepository : IBaseRepository
    {
        Task<Season> CreateSeason(CreateSeasonRequest createSeasonRequest);
        Task<bool> CancelSeasonSchedule(int seasonId);
        Task<Season> GetSeasonById(int seasonId);
        Task<bool> CloseSeasonRegistration(int seasonId);
        Task<bool> CloseSeason(int seasonId);
        Task<bool> DeleteSeasonById(int seasonId);
        Task<bool> UpdateSeasonType(int seasonId, int seasonType);
        Task<bool> UpdateSeasonSettingsId(int seasonId, int seasonSettingsId);
        Task<bool> CloseSeasonRegistration(int seasonId, int seasonType);
        Task<IEnumerable<SeasonTeamStandings>> GetSeasonTeamStandings(int seasonId);
        Task<IEnumerable<SeasonPlayerStandings>> GetSeasonPlayerStandings(int seasonId);
        Task<IEnumerable<SeasonMatchFixture>> GetSeasonMatchFixture(int seasonId);
        Task<bool> UpdateSeasonChampion(int seasonId, int championId);
        Task<int?> GetLastSeasonId();
        Task<IEnumerable<SeasonsToConclude>> GetLeaguesToConclude();
        Task<IEnumerable<SeasonsToConclude>> GetTournamentsToConclude();
        Task<IEnumerable<TournamentsToProceed>> CheckTournamentsToProceed();
        Task<Season> GetLastWeeklyLeague();
        Task<Season> GetLastWeeklyTournament();
    }
}

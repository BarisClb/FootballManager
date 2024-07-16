using FootballManager.Application.Models.DbCommands;
using FootballManager.Application.Models.Dtos;
using FootballManager.Application.Models.View;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Repositories.Dapper
{
    public interface ITeamRepository : IBaseRepository
    {
        Task<Team> CreateTeam(TeamCommand createTeamCommand);
        Task<Team> GetTeamById(int teamId);
        Task<IEnumerable<Team>> GetTeamsBySeasonId(int seasonId);
        Task<bool> DeleteTeamById(int teamId);
        Task<bool> DeleteTeamsBySeasonId(int seasonId);
        Task<Team> FindTeamBySeasonAndManager(int seasonId, int managerId);
        Task<SeasonTeamStandings> GetTeamStats(int teamId);
        Task<bool> UpdateSeasonPlacements(List<BulkUpdateSeasonPlacement> teamPlacements);
    }
}

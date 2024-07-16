using FootballManager.Application.Models.DbCommands;
using FootballManager.Application.Models.Dtos;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Repositories.Dapper
{
    public interface IMatchRepository : IBaseRepository
    {
        Task<Match> CreateMatch(MatchCommand createMatchCommand);
        Task<Match> GetMatchById(int matchId);
        Task<IEnumerable<Match>> GetMatchesBySeasonId(int seasonId);
        Task<IEnumerable<Match>> GetMatchesToSimulate();
        Task<Match> ConcludeMatch(MatchCommand createMatchCommand);
        Task<bool> DeleteMatchesForSeason(int seasonId);
        Task<IEnumerable<Match>> GetMatchesBySeasonIdAndTeamIds(int seasonId, int teamOneId, int teamTwoId);
        Task<Match> GetSeasonTieBreakerMatch(int seasonId);
        Task<bool> BulkInsertMatchesForSeason(List<BulkInsertMatch> matchList);
        Task<Match> GetFinalMatchOfTheTournament(int seasonId);
    }
}

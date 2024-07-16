using FootballManager.Application.Models.DbCommands;
using FootballManager.Application.Models.Dtos;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Repositories.Dapper
{
    public interface IPlayerRepository : IBaseRepository
    {
        Task<Player> CreatePlayer(PlayerCommand createPlayerCommand);
        Task<bool> BulkInsertPlayersForTeam(List<BulkInsertPlayer> playerList);
        Task<bool> DeletePlayersBySeasonId(int seasonId);
        Task<Player> GetPlayerById(int playerId);
        Task<IEnumerable<Player>> GetPlayersByTeamId(int teamId);
        Task<IEnumerable<PlayerListOfTheMatch>> GetPlayersByMatchId(int matchId);
    }
}

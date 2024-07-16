using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Models.DbCommands;
using FootballManager.Application.Models.Dtos;
using FootballManager.Application.Settings;
using FootballManager.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FootballManager.Persistence.Repositories.Dapper
{
    public class PlayerRepository : BaseRepository, IPlayerRepository
    {
        public PlayerRepository(IOptions<MsSqlSettings> msSqlSettings) : base(msSqlSettings)
        { }


        public async Task<Player> CreatePlayer(PlayerCommand createPlayerCommand)
        {
            string createPlayerQuery = @"INSERT INTO Players (Name, Position, Number, TeamId, SeasonId) 
                                         OUTPUT INSERTED.* 
                                         VALUES (@Name, @Position, @Number, @TeamId, @SeasonId)";
            return await QuerySingleAsync<Player>(createPlayerQuery, createPlayerCommand);
        }

        public async Task<Player> GetPlayerById(int playerId)
        {
            string getPlayerByIdQuery = @"SELECT * FROM Players WITH(NOLOCK) WHERE Id=@PlayerId";
            return await GetAsync<Player>(getPlayerByIdQuery, new { PlayerId = playerId });
        }

        public async Task<bool> DeletePlayersBySeasonId(int seasonId)
        {
            string deleteTeamById = @"DELETE FROM Players WHERE SeasonId=@SeasonId";
            return await ExecuteAsync(deleteTeamById, new { SeasonId = seasonId }) > 0;
        }

        public async Task<IEnumerable<PlayerListOfTheMatch>> GetPlayersByMatchId(int matchId)
        {
            string getMatchByIdQuery = @"SELECT ps.Id as 'PlayerId', ps.Name as 'PlayerName', ps.Position as 'PlayerPosition', ps.Number as 'PlayerNumber', ps.TeamId as 'TeamId', m.Id as 'MatchId', 
                                         (CASE WHEN ps.TeamId = t1.Id THEN t1.Name ELSE t2.Name END) as 'TeamName' 
                                         FROM Matches AS m WITH(NOLOCK) 
                                         LEFT JOIN Teams AS t1 WITH(NOLOCK) ON t1.Id = m.HomeTeamId 
                                         LEFT JOIN Teams AS t2 WITH(NOLOCK) ON t2.Id = m.AwayTeamId 
                                         LEFT JOIN Players AS ps WITH(NOLOCK) ON (ps.TeamId = t1.Id or ps.TeamId = t2.Id) 
                                         WHERE m.Id=@MatchId";
            return await GetAllAsync<PlayerListOfTheMatch>(getMatchByIdQuery, new { MatchId = matchId });
        }

        public async Task<IEnumerable<Player>> GetPlayersByTeamId(int teamId)
        {
            string getMatchByIdQuery = @"SELECT * FROM Players WITH(NOLOCK) WHERE Id=@TeamId";
            return await GetAllAsync<Player>(getMatchByIdQuery, new { TeamId = teamId });
        }

        // DapperPlus - Trial Period ends every month, instead of keep Updating the version I implemented EFC for Add-Update-Delete Range

        public async Task<bool> BulkInsertPlayersForTeam(List<BulkInsertPlayer> playerList)
        {
            await BulkInsertAsync(playerList);
            return true;
        }
    }
}

using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Models.DbCommands;
using FootballManager.Application.Settings;
using FootballManager.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FootballManager.Persistence.Repositories.Dapper
{
    public class GoalRepository : BaseRepository, IGoalRepository
    {
        public GoalRepository(IOptions<MsSqlSettings> msSqlSettings) : base(msSqlSettings)
        { }


        public async Task<Goal> CreateGoal(GoalCommand createGoalCommand)
        {
            string createGoalQuery = @"INSERT INTO Goals (MinuteScored, MatchId, TeamScoredId, TeamConcededId, ScoredById, AssistedById) 
                                       OUTPUT INSERTED.* 
                                       VALUES (@MinuteScored, @MatchId, @TeamScoredId, @TeamConcededId, @ScoredById, @AssistedById)";
            return await QuerySingleAsync<Goal>(createGoalQuery, createGoalCommand);
        }

        public async Task<Goal> GetGoalById(int goalId)
        {
            string getGoalByIdQuery = @"SELECT * FROM Goals WITH(NOLOCK) WHERE Id=@GoalId";
            return await GetAsync<Goal>(getGoalByIdQuery, new { GoalId = goalId });
        }

        public async Task<IEnumerable<Goal>> GetGoalsByMatchId(int matchId)
        {
            string getGoalsByMatchIdQuery = @"SELECT * FROM Goals WITH(NOLOCK) WHERE MatchId=@MatchId";
            return await GetAllAsync<Goal>(getGoalsByMatchIdQuery, new { MatchId = matchId });
        }

        public async Task<IEnumerable<Goal>> GetGoalsScoredByPlayerId(int playerId)
        {
            string getGoalsScoredByPlayerId = @"SELECT * FROM Goals WITH(NOLOCK) WHERE ScoredById=@PlayerId";
            return await GetAllAsync<Goal>(getGoalsScoredByPlayerId, new { PlayerId = playerId });
        }

        public async Task<IEnumerable<Goal>> GetGoalsAssistedByPlayerId(int playerId)
        {
            string getGoalsAssistedByPlayerId = @"SELECT * FROM Goals WITH(NOLOCK) WHERE AssistedById=@PlayerId";
            return await GetAllAsync<Goal>(getGoalsAssistedByPlayerId, new { PlayerId = playerId });
        }

        public async Task<bool> DeleteGoalsByMatchId(int matchId)
        {
            string deleteGoalByMatchId = @"DELETE FROM Goals WITH(NOLOCK) WHERE MatchId=@MatchId";
            return await ExecuteAsync(deleteGoalByMatchId, new { MatchId = matchId }) > 0;
        }
    }
}

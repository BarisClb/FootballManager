using FootballManager.Application.Models.DbCommands;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Repositories.Dapper
{
    public interface IGoalRepository : IBaseRepository
    {
        Task<Goal> CreateGoal(GoalCommand createGoalCommand);
        Task<Goal> GetGoalById(int goalId);
        Task<IEnumerable<Goal>> GetGoalsByMatchId(int matchId);
        Task<IEnumerable<Goal>> GetGoalsScoredByPlayerId(int playerId);
        Task<IEnumerable<Goal>> GetGoalsAssistedByPlayerId(int playerId);
        Task<bool> DeleteGoalsByMatchId(int matchId);
    }
}

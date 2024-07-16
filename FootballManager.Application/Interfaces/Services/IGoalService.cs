using FootballManager.Application.Models.DbCommands;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Services
{
    public interface IGoalService
    {
        Task<Goal> CreateGoal(GoalCommand createGoalCommand);
    }
}

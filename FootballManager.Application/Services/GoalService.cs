using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Models.DbCommands;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Services
{
    public class GoalService : IGoalService
    {
        private readonly IGoalRepository _goalRepository;

        public GoalService(IGoalRepository goalRepository)
        {
            _goalRepository = goalRepository ?? throw new ArgumentNullException(nameof(goalRepository));
        }


        public async Task<Goal> CreateGoal(GoalCommand createGoalCommand)
        {
            return await _goalRepository.CreateGoal(createGoalCommand);
        }
    }
}

using FootballManager.Application.Models.DbCommands;
using FootballManager.Application.Models.Requests;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Repositories.Dapper
{
    public interface ISeasonPassRepository : IBaseRepository
    {
        Task<SeasonPass> CreateSeasonPass(CreateSeasonPassRequest createSeasonPassRequest);
        Task<SeasonPass> UseSeasonPass(SeasonPassCommand useSeasonPassCommand);
        Task<SeasonPass> GetSeasonPassById(int seasonPassId);
        Task<SeasonPass> GetValidSeasonPassBySeasonIdAndPassword(int seasonId, string password);
    }
}

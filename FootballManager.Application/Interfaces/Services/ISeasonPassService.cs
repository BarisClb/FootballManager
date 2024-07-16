using FootballManager.Application.Models.Requests;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Services
{
    public interface ISeasonPassService
    {
        Task<SeasonPass> CreateSeasonPass(CreateSeasonPassRequest createSeasonPassRequest);
    }
}

using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Models.Requests;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Services
{
    public class SeasonPassService : ISeasonPassService
    {
        private readonly ISeasonPassRepository _seasonPassRepository;

        public SeasonPassService(ISeasonPassRepository seasonPassRepository)
        {
            _seasonPassRepository = seasonPassRepository ?? throw new ArgumentNullException(nameof(seasonPassRepository));
        }


        public async Task<SeasonPass> CreateSeasonPass(CreateSeasonPassRequest createSeasonPassRequest)
        {
            return await _seasonPassRepository.CreateSeasonPass(createSeasonPassRequest);
        }
    }
}

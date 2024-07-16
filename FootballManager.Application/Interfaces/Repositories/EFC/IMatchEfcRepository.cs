using FootballManager.Application.Models.Dtos;

namespace FootballManager.Application.Interfaces.Repositories.EFC
{
    public interface IMatchEfcRepository : IBaseEfcRepository<Domain.Entities.Match>
    {
        Task<bool> BulkInsertMatchesForSeason(List<BulkInsertMatch> matchList);
    }
}

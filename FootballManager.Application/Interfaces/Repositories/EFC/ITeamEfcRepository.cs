using FootballManager.Application.Models.Dtos;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Repositories.EFC
{
    public interface ITeamEfcRepository : IBaseEfcRepository<Team>
    {
        Task<bool> UpdateSeasonPlacements(List<BulkUpdateSeasonPlacement> teamPlacements);
    }
}

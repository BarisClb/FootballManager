using FootballManager.Application.Interfaces.Repositories.EFC;
using FootballManager.Application.Models.Dtos;
using FootballManager.Domain.Entities;
using FootballManager.Persistence.Contexts;

namespace FootballManager.Persistence.Repositories.EFC
{
    public class TeamEfcRepository : BaseEfcRepository<Team>, ITeamEfcRepository
    {
        public TeamEfcRepository(FootballManagerDbContext context) : base(context)
        { }

        public async Task<bool> UpdateSeasonPlacements(List<BulkUpdateSeasonPlacement> teamPlacements)
        {
            List<Team> teams = new();

            foreach (var placement in teamPlacements)
            {
                var team = await _entity.FindAsync(placement.Id);
                team.SeasonPlacement = placement.SeasonPlacement;

                teams.Add(team);
            }

            await UpdateRangeAsync(teams);
            return true;
        }
    }
}

using FootballManager.Application.Interfaces.Repositories.EFC;
using FootballManager.Application.Models.Dtos;
using FootballManager.Domain.Entities;
using FootballManager.Persistence.Contexts;

namespace FootballManager.Persistence.Repositories.EFC
{
    public class MatchEfcRepository : BaseEfcRepository<Domain.Entities.Match>, IMatchEfcRepository
    {
        public MatchEfcRepository(FootballManagerDbContext context) : base(context)
        { }


        public async Task<bool> BulkInsertMatchesForSeason(List<BulkInsertMatch> matchList)
        {
            List<Match> matches = new();

            foreach (var match in matchList)
            {
                Match newMatch = new();
                newMatch.MatchType = (Domain.Enums.MatchType)match.MatchType;
                newMatch.MatchSeasonPeriodType = match.MatchSeasonPeriodType;
                newMatch.SeasonRound = match.SeasonRound;
                newMatch.DatePlanned = match.DatePlanned;
                newMatch.SeasonId = (int)match.SeasonId;
                newMatch.HomeTeamManagerId = (int)match.HomeTeamManagerId;
                newMatch.HomeTeamId = (int)match.HomeTeamId;
                newMatch.AwayTeamManagerId = (int)match.AwayTeamManagerId;
                newMatch.AwayTeamId = (int)match.AwayTeamId;

                matches.Add(newMatch);
            }

            await AddRangeAsync(matches);
            return true;
        }
    }
}

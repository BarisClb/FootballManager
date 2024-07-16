using FootballManager.Domain.Enums;

namespace FootballManager.Application.Models.Dtos
{
    public class BulkInsertMatch
    {
        public Domain.Enums.MatchType? MatchType { get; set; }
        public MatchSeasonPeriodType? MatchSeasonPeriodType { get; set; }
        public int? SeasonRound { get; set; }
        public DateTime? DatePlanned { get; set; }
        public int? SeasonId { get; set; }
        public int? HomeTeamManagerId { get; set; }
        public int? HomeTeamId { get; set; }
        public int? AwayTeamManagerId { get; set; }
        public int? AwayTeamId { get; set; }
    }
}

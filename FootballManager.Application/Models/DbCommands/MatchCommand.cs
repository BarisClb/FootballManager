using FootballManager.Domain.Enums;

namespace FootballManager.Application.Models.DbCommands
{
    public class MatchCommand : BaseCommand
    {
        public Domain.Enums.MatchType? MatchType { get; set; }
        public MatchSeasonPeriodType? MatchSeasonPeriodType { get; set; }
        public int? SeasonRound { get; set; }
        public int? HomeTeamFullTimeScore { get; set; }
        public int? AwayTeamFullTimeScore { get; set; }
        public int? HomeTeamOvertimeScore { get; set; }
        public int? AwayTeamOvertimeScore { get; set; }
        public int? HomeTeamPenaltyScore { get; set; }
        public int? AwayTeamPenaltyScore { get; set; }
        public MatchResultType? Winner { get; set; }
        public DateTime? DatePlanned { get; set; }
        public DateTime? DatePlayed { get; set; }
        public int? SeasonId { get; set; }
        public int? HomeTeamManagerId { get; set; }
        public int? HomeTeamId { get; set; }
        public int? AwayTeamManagerId { get; set; }
        public int? AwayTeamId { get; set; }
    }
}

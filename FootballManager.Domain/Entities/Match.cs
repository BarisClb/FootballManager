using FootballManager.Domain.Enums;

namespace FootballManager.Domain.Entities
{
    public class Match : BaseEntity
    {
        public int SeasonId { get; set; }
        public Season Season { get; set; }

        public int? SeasonRound { get; set; }
        public DateTime? DatePlanned { get; set; }
        public DateTime? DatePlayed { get; set; }
        public MatchResultType? Winner { get; set; }

        public int HomeTeamManagerId { get; set; }
        public User HomeTeamManager { get; set; }

        public int HomeTeamId { get; set; }
        public Team HomeTeam { get; set; }

        public int? HomeTeamFullTimeScore { get; set; }
        public int? HomeTeamOvertimeScore { get; set; }
        public int? HomeTeamPenaltyScore { get; set; }
        public int? AwayTeamPenaltyScore { get; set; }
        public int? AwayTeamOvertimeScore { get; set; }
        public int? AwayTeamFullTimeScore { get; set; }

        public int AwayTeamId { get; set; }
        public Team AwayTeam { get; set; }

        public int AwayTeamManagerId { get; set; }
        public User AwayTeamManager { get; set; }

        public Enums.MatchType MatchType { get; set; }
        public MatchSeasonPeriodType? MatchSeasonPeriodType { get; set; }

        public ICollection<Goal>? Goals { get; set; }
    }
}

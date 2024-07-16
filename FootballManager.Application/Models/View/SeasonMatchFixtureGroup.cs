using FootballManager.Domain.Enums;

namespace FootballManager.Application.Models.View
{
    public class SeasonMatchFixtureGroup
    {
        public string SeasonName { get; set; }
        public int SeasonRound { get; set; }
        public List<SeasonMatchFixtureGroupMatch> Matches { get; set; } = new List<SeasonMatchFixtureGroupMatch>();
    }
    public class SeasonMatchFixtureGroupMatch
    {
        public int MatchId { get; set; }
        public int MatchType { get; set; }
        public int MatchSeasonPeriodType { get; set; }
        public int SeasonRound { get; set; }
        public DateTime MatchDatePlanned { get; set; }
        public DateTime? MatchDatePlayed { get; set; }
        public int? MatchWinner { get; set; }
        public int HomeTeamId { get; set; }
        public string HomeTeamName { get; set; }
        public int HomeTeamFullTimeScore { get; set; }
        public int? HomeTeamOvertimeScore { get; set; }
        public int? HomeTeamPenaltyScore { get; set; }
        public int HomeTeamFinalScore { get; set; }
        public int AwayTeamId { get; set; }
        public string AwayTeamName { get; set; }
        public int AwayTeamFullTimeScore { get; set; }
        public int? AwayTeamOvertimeScore { get; set; }
        public int? AwayTeamPenaltyScore { get; set; }
        public int AwayTeamFinalScore { get; set; }
        public int HomeTeamManagerId { get; set; }
        public string HomeTeamManagerName { get; set; }
        public int AwayTeamManagerId { get; set; }
        public string AwayTeamManagerName { get; set; }
        public List<SeasonMatchFixtureGroupMatchGoal> Goals { get; set; } = new List<SeasonMatchFixtureGroupMatchGoal>();
    }
    public class SeasonMatchFixtureGroupMatchGoal
    {
        public int? GoalMinuteScored { get; set; }
        public MatchSideType? MatchSideScored { get; set; }
        public int? GoalScoredById { get; set; }
        public string? GoalScoredByName { get; set; }
        public int? GoalScoredByNumber { get; set; }
        public int? GoalAssistById { get; set; }
        public string? GoalAssistByName { get; set; }
        public int? GoalAssistByNumber { get; set; }
    }
}

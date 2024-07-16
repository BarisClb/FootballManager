using FootballManager.Domain.Enums;

namespace FootballManager.Application.Models.Requests
{
    public class ScheduleSeasonRequest
    {
        public int? SeasonId { get; set; }
        public SeasonType? SeasonType { get; set; }
        public List<string>? DailyMatchHours { get; set; }
        public DateTime? FirstMatchAfter { get; set; }
        public int? MatchIntervalMinutes { get; set; }
        public int? SeasonHalvesMinimumIntervalHours { get; set; }
        public int? HomeTeamExtraGoalChance { get; set; }
        public int? TiebreakerMatchHoursAfter { get; set; }
        public bool? StartNextRoundNextDay { get; set; }
        public bool? AllowNoAssistGoals { get; set; }
        public int? NumberOfTeamsToParticipate { get; set; }
        public int? SeasonStartDay { get; set; }
    }
}

namespace FootballManager.Application.Settings
{
    public class ScheduleWeeklyLeagueSettings
    {
        public List<string>? MatchTimes { get; set; }
        public int? MatchIntervalMinutes { get; set; }
        public int? LeagueHalvesMinumumIntervalHours { get; set; }
        public int? HomeTeamExtraGoalChance { get; set; }
        public bool? AllowNoAssistGoals { get; set; }
        public int? TiebreakerMatchHoursAfter { get; set; }
        public int? NumberOfTeamsToParticipate { get; set; }
        public string? SeasonName { get; set; }
        public int? SeasonStartDay { get; set; }
    }
}

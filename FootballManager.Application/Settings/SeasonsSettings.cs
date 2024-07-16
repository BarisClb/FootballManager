namespace FootballManager.Application.Settings
{
    public class SeasonsSettings
    {
        public int? CurrentSeasonId { get; set; }
        public List<string>? DefaultMatchTimes { get; set; }
        public int? DefaultMatchIntervalMinutes { get; set; } // Time Between Matches
        public int? LeagueHalvesMinumumIntervalHours { get; set; } // Time Between First and Second Half of the League
        public int? HomeTeamExtraGoalChance { get; set; } // HomeTeamExtraGoalChance / 100
        public bool? AllowNoAssistGoals { get; set; }
        public int? TiebreakerMatchHoursAfter { get; set; } // Tiebreaker Match Scheduled After 'TiebreakerMatchHoursAfter' Hours
    }
}

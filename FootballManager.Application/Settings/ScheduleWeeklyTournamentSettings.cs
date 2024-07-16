namespace FootballManager.Application.Settings
{
    public class ScheduleWeeklyTournamentSettings
    {
        public List<string>? MatchTimes { get; set; }
        public int? MatchIntervalMinutes { get; set; }
        public bool? StartNextRoundNextDay { get; set; }
        public bool? AllowNoAssistGoals { get; set; }
        public int? NumberOfTeamsToParticipate { get; set; }
        public string? SeasonName { get; set; }
        public int? SeasonStartDay { get; set; }
    }
}

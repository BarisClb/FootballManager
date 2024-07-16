namespace FootballManager.Application.Models.View
{
    public class SeasonPlayerStandings
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int PlayerPosition { get; set; }
        public int PlayerNumber { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int MatchesPlayed { get; set; }
        public int GoalsScored { get; set; }
        public int GoalsAssisted { get; set; }
        public int GoalTotal { get; set; }
        public string SeasonName { get; set; }
    }
}

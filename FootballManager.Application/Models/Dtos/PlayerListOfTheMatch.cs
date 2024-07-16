namespace FootballManager.Application.Models.Dtos
{
    public class PlayerListOfTheMatch
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int PlayerPosition { get; set; }
        public int PlayerNumber { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int MatchId { get; set; }
    }
}

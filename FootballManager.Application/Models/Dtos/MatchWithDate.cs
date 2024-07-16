namespace FootballManager.Application.Models.Dtos
{
    public class MatchWithDate
    {
        public int Round { get; set; }
        public DateTime? Date { get; set; }
        public int Team1 { get; set; }
        public int Team2 { get; set; }
    }
}

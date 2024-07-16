namespace FootballManager.Application.Models.Dtos
{
    public class BulkInsertPlayer
    {
        public string? Name { get; set; }
        public int? Position { get; set; }
        public int? Number { get; set; }
        public int? TeamId { get; set; }
        public int? SeasonId { get; set; }
    }
}

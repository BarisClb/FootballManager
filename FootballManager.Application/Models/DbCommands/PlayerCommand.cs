namespace FootballManager.Application.Models.DbCommands
{
    public class PlayerCommand : BaseCommand
    {
        public string? Name { get; set; }
        public int? Position { get; set; }
        public int? Number { get; set; }
        public int? TeamId { get; set; }
        public int? SeasonId { get; set; }
    }
}

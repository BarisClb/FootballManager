namespace FootballManager.Application.Models.DbCommands
{
    public class TeamCommand : BaseCommand
    {
        public string? Name { get; set; }
        public int? ManagerId { get; set; }
        public int? SeasonId { get; set; }
        public int? SeasonPlacement { get; set; }
    }
}

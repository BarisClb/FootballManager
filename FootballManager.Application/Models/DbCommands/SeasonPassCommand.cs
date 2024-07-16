namespace FootballManager.Application.Models.DbCommands
{
    public class SeasonPassCommand : BaseCommand
    {
        public string? Password { get; set; }
        public DateTime? DateUsed { get; set; }
        public int? ManagerId { get; set; }
        public int? TeamId { get; set; }
        public int? SeasonId { get; set; }
    }
}

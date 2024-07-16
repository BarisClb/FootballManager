using FootballManager.Domain.Enums;

namespace FootballManager.Application.Models.DbCommands
{
    public class SeasonCommand : BaseCommand
    {
        public string? Name { get; set; }
        public SeasonType? SeasonType { get; set; }
        public bool? IsRegistrationOpen { get; set; }
        public bool? HasSeasonEnded { get; set; }
        public int? ChampionId { get; set; }
    }
}

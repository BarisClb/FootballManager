using FootballManager.Domain.Enums;

namespace FootballManager.Domain.Entities
{
    public class Season : BaseEntity
    {
        public string Name { get; set; }
        public SeasonType? SeasonType { get; set; }
        public bool IsRegistrationOpen { get; set; }
        public bool HasSeasonEnded { get; set; }

        public int? ChampionId { get; set; }
        public Team? Champion { get; set; }

        public int? SeasonSettingsId { get; set; }
        public SeasonSettings? SeasonSettings { get; set; }

        public ICollection<Player>? Players { get; set; }

        public ICollection<Team>? Teams { get; set; }

        public ICollection<Match>? Matches { get; set; }

        public ICollection<SeasonPass>? SeasonPasses { get; set; }
    }
}

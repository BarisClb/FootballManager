namespace FootballManager.Domain.Entities
{
    public class Team : BaseEntity
    {
        public string Name { get; set; }

        public int ManagerId { get; set; }
        public User Manager { get; set; }

        public int SeasonId { get; set; }
        public Season Season { get; set; }

        public int? SeasonPlacement { get; set; }

        public ICollection<Goal>? Scored { get; }
        public ICollection<Goal>? Conceded { get; }

        public ICollection<Match>? HomeMatches { get; }
        public ICollection<Match>? AwayMatches { get; }

        public ICollection<Player>? Players { get; set; }

        public ICollection<Season>? ChampionSeasons { get; set; }

        public ICollection<SeasonPass>? SeasonPassess { get; set; }
    }
}

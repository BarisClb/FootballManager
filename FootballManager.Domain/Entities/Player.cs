namespace FootballManager.Domain.Entities
{
    public class Player : BaseEntity
    {
        public string Name { get; set; }
        public int Position { get; set; }
        public int Number { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; }

        public int SeasonId { get; set; }
        public Season Season { get; set; }

        public ICollection<Goal>? Scored { get; }

        public ICollection<Goal>? Assisted { get; }
    }
}

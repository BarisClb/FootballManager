namespace FootballManager.Domain.Entities
{
    public class Goal : BaseEntity
    {
        public int MatchId { get; set; }
        public Match Match { get; set; }

        public int MinuteScored { get; set; }

        public int TeamScoredId { get; set; }
        public Team TeamScored { get; set; }

        public int ScoredById { get; set; }
        public Player ScoredBy { get; set; }

        public int? AssistedById { get; set; }
        public Player? AssistedBy { get; set; }

        public int TeamConcededId { get; set; }
        public Team TeamConceded { get; set; }
    }
}

namespace FootballManager.Domain.Entities
{
    public class SeasonPass : BaseEntity
    {
        public string Password { get; set; }
        public DateTime? DateUsed { get; set; }

        public int? ManagerId { get; set; }
        public User? Manager { get; set; }

        public int? TeamId { get; set; }
        public Team? Team { get; set; }

        public int SeasonId { get; set; }
        public Season Season { get; set; }
    }
}

namespace FootballManager.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
        public string? Groups { get; set; }
        public string? Roles { get; set; }
        public ICollection<Team>? Teams { get; set; }
        public ICollection<SeasonPass>? SeasonPasses { get; set; }
        public ICollection<Match>? HomeMatches { get; set; }
        public ICollection<Match>? AwayMatches { get; set; }
    }
}

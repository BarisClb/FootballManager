namespace FootballManager.Application.Settings
{
    public class HangfireSettings
    {
        public string? ConnectionString { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? SimulateMatchesJobCronTime { get; set; }
    }
}

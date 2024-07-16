namespace FootballManager.Application.Models.DbCommands
{
    public class GoalCommand : BaseCommand
    {
        public int? MinuteScored { get; set; }
        public int? MatchId { get; set; }
        public int? TeamScoredId { get; set; }
        public int? TeamConcededId { get; set; }
        public int? ScoredById { get; set; }
        public int? AssistedById { get; set; }
    }
}

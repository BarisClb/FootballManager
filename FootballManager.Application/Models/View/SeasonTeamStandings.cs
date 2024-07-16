namespace FootballManager.Application.Models.View
{
    public class SeasonTeamStandings
    {
        public string SeasonName { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int? SeasonPlacement { get; set; }
        public int ManagerId { get; set; }
        public string ManagerName { get; set; }
        public int MatchesPlayedTotal { get; set; }
        public int PointTotal { get; set; }
        public int ScoredTotal { get; set; }
        public int ConcededTotal { get; set; }
        public int MatchesWonTotal { get; set; }
        public int MatchesDrawTotal { get; set; }
        public int MatchesLostTotal { get; set; }
        public int GoalAvarageTotal { get; set; }

        public int HomeMatchesPlayed { get; set; }
        public int HomePointTotal { get; set; }
        public int HomeMatchesWon { get; set; }
        public int HomeMatchesDraw { get; set; }
        public int HomeMatchesLost { get; set; }
        public int HomeFullTimeScoreSum { get; set; }
        public int HomeOvertimeScoreSum { get; set; }
        public int HomeScoreSum { get; set; }
        public int HomeFullTimeConcededSum { get; set; }
        public int HomeOvertimeConcededSum { get; set; }
        public int HomeConcededSum { get; set; }
        public int HomeGoalAvarage { get; set; }

        public int AwayMatchesPlayed { get; set; }
        public int AwayPointTotal { get; set; }
        public int AwayMatchesWon { get; set; }
        public int AwayMatchesDraw { get; set; }
        public int AwayMatchesLost { get; set; }
        public int AwayFullTimeScoreSum { get; set; }
        public int AwayOvertimeScoreSum { get; set; }
        public int AwayScoreSum { get; set; }
        public int AwayFullTimeConcededSum { get; set; }
        public int AwayOvertimeConcededSum { get; set; }
        public int AwayConcededSum { get; set; }
        public int AwayGoalAvarage { get; set; }
    }
}

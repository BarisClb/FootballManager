namespace FootballManager.Application.Settings.Chances
{
    public static class TeamChancesSettings
    {
        // Number of Goals - Chance to Score
        public static List<(int numberOfGoals, int chanceToScore)> GoalChances = new()
        {
            (0, 17),
            (1, 15),
            (2, 13),
            (3, 11),
            (4, 9),
            (5, 7),
            (6, 5),
            (7, 3),
            (8, 1)
        };
    }
}

namespace FootballManager.Application.Settings.Chances
{
    // Position of Player - Chance To Score
    public static class PlayerChancesSettings
    {
        public static List<(int playerPosition, int chanceToScore)> ChancesToScore = new()
        {
            // Goalkeeper
            (1, 1),
            // Defence
            (2, 6),
            (3, 6),
            (4, 6),
            (5, 6),
            // Middlefield
            (6, 15),
            (7, 15),
            (8, 15),
            (9, 15),
            // Forward
            (10, 25),
            (11, 25)
        };

        public static List<(int playerPosition, int chanceToAssist)> ChancesToAssist = new()
        {
            // Goalkeeper
            (1, 1),
            // Defence
            (2, 7),
            (3, 7),
            (4, 7),
            (5, 7),
            // Middlefield
            (6, 20),
            (7, 20),
            (8, 20),
            (9, 20),
            // Forward
            (10, 15),
            (11, 15)
        };
    }
}

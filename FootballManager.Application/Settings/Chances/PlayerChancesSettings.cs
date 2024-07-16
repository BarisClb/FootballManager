namespace FootballManager.Application.Settings.Chances;

// Supply your own probability settings. Original weights are withheld.
public static class PlayerChancesSettings
{
    public static List<(int playerPosition, int chanceToScore)> ChancesToScore = new();
    public static List<(int playerPosition, int chanceToAssist)> ChancesToAssist = new();
}

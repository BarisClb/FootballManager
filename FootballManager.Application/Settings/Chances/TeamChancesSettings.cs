namespace FootballManager.Application.Settings.Chances;

// Supply your own probability settings. Original weights are withheld.
public static class TeamChancesSettings
{
    public static List<(int numberOfGoals, int chanceToScore)> GoalChances = new();
}

namespace FootballManager.Application.Helpers
{
    public static class IntHelper
    {
        public static int Factorial(this int number)
        {
            if (number == 0)
                return 1;
            else
                return number * Factorial(number - 1);
        }
    }
}

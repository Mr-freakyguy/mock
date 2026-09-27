namespace CoffeeMachine.View
{
    public class ConsoleHelper
    {
        private const int MaxAttempts = 3;

        public static int GetIntegerInput(string prompt, int min, int max)
        {
            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                if (!string.IsNullOrEmpty(prompt)) Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int input) && input >= min && input <= max)
                {
                    return input;
                }
                Console.WriteLine($"Invalid input. Please enter an integer between {min} and {max}. ({MaxAttempts - attempt} attempts left)");
            }
            return -1;
        }

        public static string GetNonEmptyString(string prompt)
        {
            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input;
                }
                Console.WriteLine($"Input cannot be empty. Please try again. ({MaxAttempts - attempt} attempts left)");
            }
            return string.Empty;
        }
    }
}

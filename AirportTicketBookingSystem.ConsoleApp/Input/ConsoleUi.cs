
namespace AirportTicketBookingSystem.ConsoleApp.Input
{
    public static class ConsoleUi
    {
        public static void Header(string title)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("============================================================");
            Console.WriteLine($"  {title}");
            Console.WriteLine("============================================================");
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void Section(string title)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine($"--- {title} ---");
            Console.ResetColor();
        }

        public static string Prompt(string label)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{label}: ");
            Console.ResetColor();
            return Console.ReadLine() ?? string.Empty;
        }

        public static void Success(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void Pause()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Press any key to continue...");
            Console.ResetColor();
            Console.ReadKey();
        }
    }
}

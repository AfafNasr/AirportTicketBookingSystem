

using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.ConsoleApp.Input
{
    public static class ConsoleParsers
    {
        public static decimal? ReadNullableDecimal(string label)
        {
            var input = ConsoleUi.Prompt(label);

            if (string.IsNullOrWhiteSpace(input))
                return null;

            return decimal.TryParse(input, out var value) ? value : null;
        }

        public static DateTime? ReadNullableDate(string label)
        {
            var input = ConsoleUi.Prompt(label);

            if (string.IsNullOrWhiteSpace(input))
                return null;

            return DateTime.TryParse(input, out var value) ? value : null;
        }

        public static TravelClass? ReadNullableTravelClass()
        {
            Console.WriteLine("Travel Class:");
            Console.WriteLine("1. Economy");
            Console.WriteLine("2. Business");
            Console.WriteLine("3. First Class");
            Console.WriteLine("Leave empty for all");

            var input = ConsoleUi.Prompt("Choose class");

            return input switch
            {
                "1" => TravelClass.Economy,
                "2" => TravelClass.Business,
                "3" => TravelClass.FirstClass,
                "" => null,
                _ => null
            };
        }

        public static TravelClass ReadRequiredTravelClass()
        {
            while (true)
            {
                Console.WriteLine("Travel Class:");
                Console.WriteLine("1. Economy");
                Console.WriteLine("2. Business");
                Console.WriteLine("3. First Class");

                var input = ConsoleUi.Prompt("Choose class");

                var travelClass = input switch
                {
                    "1" => TravelClass.Economy,
                    "2" => TravelClass.Business,
                    "3" => TravelClass.FirstClass,
                    _ => (TravelClass?)null
                };

                if (travelClass.HasValue)
                    return travelClass.Value;

                ConsoleUi.Error("Invalid class selection.");
            }
        }
    }
}


using AirportTicketBookingSystem.Application.DTOs.Flights;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Input;
using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.ConsoleApp.Workflows.Passenger
{
    public sealed class PassengerFlightSearchWorkflow
    {
        private readonly FlightService _flightService;

        public PassengerFlightSearchWorkflow(FlightService flightService)
        {
            _flightService = flightService;
        }

        public async Task<IReadOnlyList<FlightSearchResult>> SearchFlightsWithoutPauseAsync()
        {
            ConsoleUi.Section("Guided Search");

            var allFlights = await _flightService.SearchAvailableFlightsAsync(new FlightSearchRequest());

            if (allFlights.Count == 0)
            {
                ConsoleUi.Info("No flights are available in the system.");
                return [];
            }

            var departureCountry = ChooseChipOption(
                "Departure Country",
                allFlights
                    .Select(f => f.DepartureCountry)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList());

            var destinationCountry = ChooseChipOption(
                "Destination Country",
                allFlights
                    .Where(f => IsEmptyOrEqual(departureCountry, f.DepartureCountry))
                    .Select(f => f.DestinationCountry)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList());

            var departureDateText = ChooseChipOption(
                "Departure Date",
                allFlights
                    .Where(f =>
                        IsEmptyOrEqual(departureCountry, f.DepartureCountry) &&
                        IsEmptyOrEqual(destinationCountry, f.DestinationCountry))
                    .Select(f => f.DepartureDate.ToString("yyyy-MM-dd"))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList());

            DateTime? departureDate = string.IsNullOrWhiteSpace(departureDateText)
                ? null
                : DateTime.Parse(departureDateText);

            var departureAirport = ChooseChipOption(
                "Departure Airport",
                allFlights
                    .Where(f =>
                        IsEmptyOrEqual(departureCountry, f.DepartureCountry) &&
                        IsEmptyOrEqual(destinationCountry, f.DestinationCountry) &&
                        (!departureDate.HasValue || f.DepartureDate.Date == departureDate.Value.Date))
                    .Select(f => f.DepartureAirport)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList());

            var arrivalAirport = ChooseChipOption(
                "Arrival Airport",
                allFlights
                    .Where(f =>
                        IsEmptyOrEqual(departureCountry, f.DepartureCountry) &&
                        IsEmptyOrEqual(destinationCountry, f.DestinationCountry) &&
                        (!departureDate.HasValue || f.DepartureDate.Date == departureDate.Value.Date) &&
                        IsEmptyOrEqual(departureAirport, f.DepartureAirport))
                    .Select(f => f.ArrivalAirport)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList());

            var travelClassText = ChooseChipOption(
                "Travel Class",
                ["Economy", "Business", "FirstClass"]);

            var travelClass = travelClassText switch
            {
                "Economy" => TravelClass.Economy,
                "Business" => TravelClass.Business,
                "FirstClass" => TravelClass.FirstClass,
                _ => (TravelClass?)null
            };

            var maxPrice = ConsoleParsers.ReadNullableDecimal("Max Price (leave empty for any)");

            var request = new FlightSearchRequest
            {
                DepartureCountry = departureCountry,
                DestinationCountry = destinationCountry,
                DepartureDate = departureDate,
                DepartureAirport = departureAirport,
                ArrivalAirport = arrivalAirport,
                TravelClass = travelClass,
                MaxPrice = maxPrice
            };

            var flights = await _flightService.SearchAvailableFlightsAsync(request);

            return flights;
        }

        private static string? ChooseChipOption(string title, IReadOnlyList<string> options)
        {
            ConsoleUi.Section(title);

            if (options.Count == 0)
            {
                ConsoleUi.Info("No options available.");
                return null;
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Leave empty for Any / Skip");
            Console.ResetColor();

            PrintChips(options);

            while (true)
            {
                var input = ConsoleUi.Prompt("Selection").Trim();

                if (string.IsNullOrWhiteSpace(input))
                    return null;

                var selected = options.FirstOrDefault(option =>
                    option.Equals(input, StringComparison.OrdinalIgnoreCase));

                if (selected is not null)
                    return selected;

                ConsoleUi.Error("Invalid selection. Please type one of the displayed options exactly.");
            }
        }

        private static void PrintChips(IReadOnlyList<string> options)
        {
            const int maxLineWidth = 85;
            var currentLineLength = 0;

            Console.WriteLine();

            foreach (var option in options)
            {
                var chip = $"[ {option} ] ";

                if (currentLineLength + chip.Length > maxLineWidth)
                {
                    Console.WriteLine();
                    currentLineLength = 0;
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(chip);
                Console.ResetColor();

                currentLineLength += chip.Length;
            }

            Console.WriteLine();
            Console.WriteLine();
        }

        private static bool IsEmptyOrEqual(string? selectedValue, string actualValue)
        {
            return string.IsNullOrWhiteSpace(selectedValue) ||
                   actualValue.Equals(selectedValue, StringComparison.OrdinalIgnoreCase);
        }
    }
}

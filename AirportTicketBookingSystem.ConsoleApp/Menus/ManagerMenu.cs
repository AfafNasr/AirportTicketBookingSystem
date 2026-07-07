using AirportTicketBookingSystem.Application.DTOs.Bookings;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Input;
using AirportTicketBookingSystem.Domain.Enums;
using AirportTicketBookingSystem.ConsoleApp.Handlers.Manager;

namespace AirportTicketBookingSystem.ConsoleApp.Menus
{
    public sealed class ManagerMenu
    {
        private readonly BookingService _bookingService;
        private readonly AuthService _authService;
        private readonly ManagerFlightImportHandler _flightImportHandler;
        private readonly ManagerValidationRulesHandler _validationRulesHandler;

        public ManagerMenu(
            BookingService bookingService,
            AuthService authService,
            ManagerFlightImportHandler flightImportHandler,
            ManagerValidationRulesHandler validationRulesHandler)
        {
            _bookingService = bookingService;
            _authService = authService;
            _flightImportHandler = flightImportHandler;
            _validationRulesHandler = validationRulesHandler;
        }

        public async Task ShowAsync()
        {
            while (true)
            {
                ConsoleUi.Header("MANAGER MENU");

                Console.WriteLine("1. Import Flights From CSV");
                Console.WriteLine("2. View Flight Validation Rules");
                Console.WriteLine("3. Filter Bookings");
                Console.WriteLine("4. Logout");
                Console.WriteLine();

                var choice = ConsoleUi.Prompt("Choose option");

                switch (choice)
                {
                    case "1":
                        await _flightImportHandler.ImportFlightsAsync();
                        break;

                    case "2":
                        _validationRulesHandler.ViewValidationRules();
                        break;

                    case "3":
                        await FilterBookingsAsync();
                        break;

                    case "4":
                        _authService.Logout();
                        return;

                    default:
                        ConsoleUi.Error("Invalid option.");
                        ConsoleUi.Pause();
                        break;
                }
            }
        }

        private async Task FilterBookingsAsync()
        {
            ConsoleUi.Header("FILTER BOOKINGS");

            var allBookings = await _bookingService.FilterBookingsAsync(new BookingFilterRequest());

            if (allBookings.Count == 0)
            {
                ConsoleUi.Info("No bookings exist in the system.");
                ConsoleUi.Pause();
                return;
            }

            var flightNumber = ChooseChipOption(
                "Flight Number",
                allBookings
                    .Select(booking => booking.FlightNumber)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value)
                    .ToList());

            var departureCountry = ChooseChipOption(
                "Departure Country",
                allBookings
                    .Where(booking => IsEmptyOrEqual(flightNumber, booking.FlightNumber))
                    .Select(booking => booking.DepartureCountry)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value)
                    .ToList());

            var destinationCountry = ChooseChipOption(
                "Destination Country",
                allBookings
                    .Where(booking =>
                        IsEmptyOrEqual(flightNumber, booking.FlightNumber) &&
                        IsEmptyOrEqual(departureCountry, booking.DepartureCountry))
                    .Select(booking => booking.DestinationCountry)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value)
                    .ToList());

            var departureDateText = ChooseChipOption(
                "Departure Date",
                allBookings
                    .Where(booking =>
                        IsEmptyOrEqual(flightNumber, booking.FlightNumber) &&
                        IsEmptyOrEqual(departureCountry, booking.DepartureCountry) &&
                        IsEmptyOrEqual(destinationCountry, booking.DestinationCountry))
                    .Select(booking => booking.DepartureDate.ToString("yyyy-MM-dd"))
                    .Distinct()
                    .OrderBy(value => value)
                    .ToList());

            DateTime? departureDate = string.IsNullOrWhiteSpace(departureDateText)
                ? null
                : DateTime.Parse(departureDateText);

            var departureAirport = ChooseChipOption(
                "Departure Airport",
                allBookings
                    .Where(booking =>
                        IsEmptyOrEqual(flightNumber, booking.FlightNumber) &&
                        IsEmptyOrEqual(departureCountry, booking.DepartureCountry) &&
                        IsEmptyOrEqual(destinationCountry, booking.DestinationCountry) &&
                        (!departureDate.HasValue || booking.DepartureDate.Date == departureDate.Value.Date))
                    .Select(booking => booking.DepartureAirport)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value)
                    .ToList());

            var arrivalAirport = ChooseChipOption(
                "Arrival Airport",
                allBookings
                    .Where(booking =>
                        IsEmptyOrEqual(flightNumber, booking.FlightNumber) &&
                        IsEmptyOrEqual(departureCountry, booking.DepartureCountry) &&
                        IsEmptyOrEqual(destinationCountry, booking.DestinationCountry) &&
                        (!departureDate.HasValue || booking.DepartureDate.Date == departureDate.Value.Date) &&
                        IsEmptyOrEqual(departureAirport, booking.DepartureAirport))
                    .Select(booking => booking.ArrivalAirport)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value)
                    .ToList());

            var passengerEmail = ChooseChipOption(
                "Passenger Email",
                allBookings
                    .Select(booking => booking.PassengerEmail)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value)
                    .ToList());

            var travelClass = ChooseTravelClassChip();

            var maxPrice = ConsoleParsers.ReadNullableDecimal("Max Price (leave empty for any)");

            var results = await _bookingService.FilterBookingsAsync(new BookingFilterRequest
            {
                FlightNumber = flightNumber,
                MaxPrice = maxPrice,
                DepartureCountry = departureCountry,
                DestinationCountry = destinationCountry,
                DepartureDate = departureDate,
                DepartureAirport = departureAirport,
                ArrivalAirport = arrivalAirport,
                PassengerEmail = passengerEmail,
                TravelClass = travelClass
            });

            PrintManagerBookingResults(results);

            ConsoleUi.Pause();
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

                ConsoleUi.Error("Invalid selection. Please type one of the displayed options.");
            }
        }

        private static TravelClass? ChooseTravelClassChip()
        {
            var selected = ChooseChipOption(
                "Travel Class",
                ["Economy", "Business", "FirstClass"]);

            return selected switch
            {
                "Economy" => TravelClass.Economy,
                "Business" => TravelClass.Business,
                "FirstClass" => TravelClass.FirstClass,
                _ => null
            };
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

        private static void PrintManagerBookingResults(IReadOnlyList<ManagerBookingResult> results)
        {
            ConsoleUi.Section("Booking Results");

            if (results.Count == 0)
            {
                ConsoleUi.Info("No bookings matched the selected filters.");
                return;
            }

            var activeCount = results.Count(booking => booking.Status == BookingStatus.Active);
            var cancelledCount = results.Count(booking => booking.Status == BookingStatus.Cancelled);
            var totalRevenue = results
                .Where(booking => booking.Status == BookingStatus.Active)
                .Sum(booking => booking.Price);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Found Bookings : {results.Count}");
            Console.WriteLine($"Active         : {activeCount}");
            Console.WriteLine($"Cancelled      : {cancelledCount}");
            Console.WriteLine($"Total Revenue  : {totalRevenue}");
            Console.ResetColor();

            Console.WriteLine();

            for (var i = 0; i < results.Count; i++)
            {
                var booking = results[i];

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Booking #{i + 1}");
                Console.ResetColor();

                Console.WriteLine(new string('-', 70));

                Console.WriteLine($"Passenger   : {booking.PassengerName}");
                Console.WriteLine($"Email       : {booking.PassengerEmail}");
                Console.WriteLine($"Flight      : {booking.FlightNumber}");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Route       : ");
                Console.ResetColor();
                Console.WriteLine($"{booking.DepartureCountry} -> {booking.DestinationCountry}");

                Console.WriteLine($"Airports    : {booking.DepartureAirport} -> {booking.ArrivalAirport}");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Departure   : ");
                Console.ResetColor();
                Console.WriteLine($"{booking.DepartureDate:yyyy-MM-dd HH:mm}");

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("Class       : ");
                Console.ResetColor();
                Console.WriteLine(booking.TravelClass);

                Console.WriteLine($"Price       : {booking.Price}");

                Console.Write("Status      : ");
                Console.ForegroundColor = booking.Status == BookingStatus.Active
                    ? ConsoleColor.Green
                    : ConsoleColor.Red;
                Console.WriteLine(booking.Status);
                Console.ResetColor();

                Console.WriteLine(new string('=', 70));
                Console.WriteLine();
            }
        }
    }
}

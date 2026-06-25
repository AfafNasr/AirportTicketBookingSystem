using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Application.DTOs.Bookings;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Input;

namespace AirportTicketBookingSystem.ConsoleApp.Menus
{
    public sealed class ManagerMenu
    {
        private readonly CsvFlightImportService _csvFlightImportService;
        private readonly FlightValidationMetadataService _validationMetadataService;
        private readonly BookingService _bookingService;
        private readonly AuthService _authService;

        public ManagerMenu(
            CsvFlightImportService csvFlightImportService,
            FlightValidationMetadataService validationMetadataService,
            BookingService bookingService,
            AuthService authService)
        {
            _csvFlightImportService = csvFlightImportService;
            _validationMetadataService = validationMetadataService;
            _bookingService = bookingService;
            _authService = authService;
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
                        await ImportFlightsAsync();
                        break;

                    case "2":
                        ViewValidationRules();
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

        private async Task ImportFlightsAsync()
        {
            ConsoleUi.Header("IMPORT FLIGHTS FROM CSV");

            var filePath = ConsoleUi.Prompt("CSV File Path");

            var result = await _csvFlightImportService.ImportAsync(filePath);

            Console.WriteLine();
            ConsoleUi.Success($"Imported Flights: {result.ImportedCount}");

            if (result.HasErrors)
            {
                ConsoleUi.Section("Validation Errors");

                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Row {error.RowNumber} | {error.FieldName}: {error.ErrorMessage}");
                }
            }

            ConsoleUi.Pause();
        }

        private void ViewValidationRules()
        {
            ConsoleUi.Header("FLIGHT VALIDATION RULES");

            var rules = _validationMetadataService.GetFlightValidationRules();

            foreach (var rule in rules)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(rule.FieldName);
                Console.ResetColor();

                Console.WriteLine($"Type: {rule.FieldType}");
                Console.WriteLine("Constraints:");

                foreach (var constraint in rule.Constraints)
                {
                    Console.WriteLine($" - {constraint}");
                }

                Console.WriteLine();
            }

            ConsoleUi.Pause();
        }

        private async Task FilterBookingsAsync()
        {
            ConsoleUi.Header("FILTER BOOKINGS");

            var request = new BookingFilterRequest
            {
                FlightNumber = ConsoleUi.Prompt("Flight Number"),
                MaxPrice = ConsoleParsers.ReadNullableDecimal("Max Price"),
                DepartureCountry = ConsoleUi.Prompt("Departure Country"),
                DestinationCountry = ConsoleUi.Prompt("Destination Country"),
                DepartureDate = ConsoleParsers.ReadNullableDate("Departure Date"),
                DepartureAirport = ConsoleUi.Prompt("Departure Airport"),
                ArrivalAirport = ConsoleUi.Prompt("Arrival Airport"),
                PassengerEmail = ConsoleUi.Prompt("Passenger Email"),
                TravelClass = ConsoleParsers.ReadNullableTravelClass()
            };

            var results = await _bookingService.FilterBookingsAsync(request);

            ConsoleUi.Section("Booking Results");

            if (results.Count == 0)
            {
                ConsoleUi.Info("No bookings found.");
                ConsoleUi.Pause();
                return;
            }

            foreach (var booking in results)
            {
                Console.WriteLine(
                    $"{booking.BookingId} | {booking.PassengerEmail} | {booking.FlightNumber} | " +
                    $"{booking.DepartureCountry}->{booking.DestinationCountry} | {booking.TravelClass} | {booking.Price} | {booking.Status}");
            }

            ConsoleUi.Pause();
        }
    }
}

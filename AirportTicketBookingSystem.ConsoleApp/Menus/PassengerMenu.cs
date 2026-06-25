using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Application.DTOs.Bookings;
using AirportTicketBookingSystem.Application.DTOs.Flights;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Input;

namespace AirportTicketBookingSystem.ConsoleApp.Menus
{
    public sealed class PassengerMenu
    {
        private readonly FlightService _flightService;
        private readonly BookingService _bookingService;
        private readonly AuthService _authService;

        public PassengerMenu(
            FlightService flightService,
            BookingService bookingService,
            AuthService authService)
        {
            _flightService = flightService;
            _bookingService = bookingService;
            _authService = authService;
        }

        public async Task ShowAsync()
        {
            while (true)
            {
                ConsoleUi.Header("PASSENGER MENU");

                Console.WriteLine("1. Search Available Flights");
                Console.WriteLine("2. Book Flight");
                Console.WriteLine("3. View My Bookings");
                Console.WriteLine("4. Cancel Booking");
                Console.WriteLine("5. Modify Booking");
                Console.WriteLine("6. Logout");
                Console.WriteLine();

                var choice = ConsoleUi.Prompt("Choose option");

                switch (choice)
                {
                    case "1":
                        await SearchFlightsAsync();
                        break;

                    case "2":
                        await BookFlightAsync();
                        break;

                    case "3":
                        await ViewMyBookingsAsync();
                        break;

                    case "4":
                        await CancelBookingAsync();
                        break;

                    case "5":
                        await ModifyBookingAsync();
                        break;

                    case "6":
                        _authService.Logout();
                        return;

                    default:
                        ConsoleUi.Error("Invalid option.");
                        ConsoleUi.Pause();
                        break;
                }
            }
        }

        private async Task<IReadOnlyList<FlightSearchResult>> SearchFlightsAsync()
        {
            ConsoleUi.Header("SEARCH AVAILABLE FLIGHTS");

            var request = new FlightSearchRequest
            {
                MaxPrice = ConsoleParsers.ReadNullableDecimal("Max Price"),
                DepartureCountry = ConsoleUi.Prompt("Departure Country"),
                DestinationCountry = ConsoleUi.Prompt("Destination Country"),
                DepartureDate = ConsoleParsers.ReadNullableDate("Departure Date"),
                DepartureAirport = ConsoleUi.Prompt("Departure Airport"),
                ArrivalAirport = ConsoleUi.Prompt("Arrival Airport"),
                TravelClass = ConsoleParsers.ReadNullableTravelClass()
            };

            var flights = await _flightService.SearchAvailableFlightsAsync(request);

            PrintFlights(flights);

            ConsoleUi.Pause();
            return flights;
        }

        private async Task BookFlightAsync()
        {
            ConsoleUi.Header("BOOK FLIGHT");

            var flights = await SearchFlightsWithoutPauseAsync();

            if (flights.Count == 0)
            {
                ConsoleUi.Error("No available flights found.");
                ConsoleUi.Pause();
                return;
            }

            var indexInput = ConsoleUi.Prompt("Select flight number");
            if (!int.TryParse(indexInput, out var index) || index < 1 || index > flights.Count)
            {
                ConsoleUi.Error("Invalid flight selection.");
                ConsoleUi.Pause();
                return;
            }

            var selectedFlight = flights[index - 1];

            var result = await _bookingService.BookFlightAsync(new BookFlightRequest
            {
                FlightId = selectedFlight.FlightId,
                TravelClass = selectedFlight.TravelClass
            });

            if (result.IsFailure)
                ConsoleUi.Error(result.Error);
            else
                ConsoleUi.Success($"Booking completed. Booking ID: {result.Value!.BookingId}");

            ConsoleUi.Pause();
        }

        private async Task<IReadOnlyList<FlightSearchResult>> SearchFlightsWithoutPauseAsync()
        {
            ConsoleUi.Section("Search Filters");

            var request = new FlightSearchRequest
            {
                MaxPrice = ConsoleParsers.ReadNullableDecimal("Max Price"),
                DepartureCountry = ConsoleUi.Prompt("Departure Country"),
                DestinationCountry = ConsoleUi.Prompt("Destination Country"),
                DepartureDate = ConsoleParsers.ReadNullableDate("Departure Date"),
                DepartureAirport = ConsoleUi.Prompt("Departure Airport"),
                ArrivalAirport = ConsoleUi.Prompt("Arrival Airport"),
                TravelClass = ConsoleParsers.ReadNullableTravelClass()
            };

            var flights = await _flightService.SearchAvailableFlightsAsync(request);
            PrintFlights(flights);

            return flights;
        }

        private async Task ViewMyBookingsAsync()
        {
            ConsoleUi.Header("MY BOOKINGS");

            var bookings = await _bookingService.GetMyBookingsAsync();

            if (bookings.Count == 0)
            {
                ConsoleUi.Info("You have no bookings.");
                ConsoleUi.Pause();
                return;
            }

            foreach (var booking in bookings)
            {
                Console.WriteLine($"{booking.BookingId} | {booking.FlightNumber} | {booking.TravelClass} | {booking.Price} | {booking.Status}");
            }

            ConsoleUi.Pause();
        }

        private async Task CancelBookingAsync()
        {
            ConsoleUi.Header("CANCEL BOOKING");

            await ViewMyBookingsShortAsync();

            var input = ConsoleUi.Prompt("Booking ID");

            if (!Guid.TryParse(input, out var bookingId))
            {
                ConsoleUi.Error("Invalid booking ID.");
                ConsoleUi.Pause();
                return;
            }

            var result = await _bookingService.CancelBookingAsync(bookingId);

            if (result.IsFailure)
                ConsoleUi.Error(result.Error);
            else
                ConsoleUi.Success("Booking cancelled successfully.");

            ConsoleUi.Pause();
        }

        private async Task ModifyBookingAsync()
        {
            ConsoleUi.Header("MODIFY BOOKING");

            await ViewMyBookingsShortAsync();

            var bookingInput = ConsoleUi.Prompt("Booking ID to modify");

            if (!Guid.TryParse(bookingInput, out var bookingId))
            {
                ConsoleUi.Error("Invalid booking ID.");
                ConsoleUi.Pause();
                return;
            }

            ConsoleUi.Section("Choose New Flight");
            var flights = await SearchFlightsWithoutPauseAsync();

            if (flights.Count == 0)
            {
                ConsoleUi.Error("No flights found.");
                ConsoleUi.Pause();
                return;
            }

            var indexInput = ConsoleUi.Prompt("Select new flight number");

            if (!int.TryParse(indexInput, out var index) || index < 1 || index > flights.Count)
            {
                ConsoleUi.Error("Invalid flight selection.");
                ConsoleUi.Pause();
                return;
            }

            var selectedFlight = flights[index - 1];

            var result = await _bookingService.ModifyBookingAsync(new ModifyBookingRequest
            {
                BookingId = bookingId,
                NewFlightId = selectedFlight.FlightId,
                NewTravelClass = selectedFlight.TravelClass
            });

            if (result.IsFailure)
                ConsoleUi.Error(result.Error);
            else
                ConsoleUi.Success("Booking modified successfully.");

            ConsoleUi.Pause();
        }

        private async Task ViewMyBookingsShortAsync()
        {
            var bookings = await _bookingService.GetMyBookingsAsync();

            if (bookings.Count == 0)
            {
                ConsoleUi.Info("You have no bookings.");
                return;
            }

            foreach (var booking in bookings)
            {
                Console.WriteLine($"{booking.BookingId} | {booking.FlightNumber} | {booking.TravelClass} | {booking.Price} | {booking.Status}");
            }
        }

        private static void PrintFlights(IReadOnlyList<FlightSearchResult> flights)
        {
            ConsoleUi.Section("Available Flights");

            if (flights.Count == 0)
            {
                ConsoleUi.Info("No flights found.");
                return;
            }

            for (var i = 0; i < flights.Count; i++)
            {
                var flight = flights[i];

                Console.WriteLine(
                    $"{i + 1}. {flight.FlightNumber} | {flight.DepartureCountry} -> {flight.DestinationCountry} | " +
                    $"{flight.DepartureDate:g} | {flight.TravelClass} | {flight.Price} | Seats: {flight.AvailableSeats}");
            }
        }
    }
}

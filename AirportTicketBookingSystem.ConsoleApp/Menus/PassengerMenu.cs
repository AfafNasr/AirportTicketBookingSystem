using AirportTicketBookingSystem.Application.DTOs.Bookings;
using AirportTicketBookingSystem.Application.DTOs.Flights;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Input;
using AirportTicketBookingSystem.Domain.Enums;
using AirportTicketBookingSystem.ConsoleApp.Workflows.Passenger;

namespace AirportTicketBookingSystem.ConsoleApp.Menus;

public sealed class PassengerMenu
{
  
    private readonly AuthService _authService;
    private readonly PassengerBookingCancellationWorkflow _bookingCancellationWorkflow;
    private readonly PassengerBookingViewerWorkflow _bookingViewerWorkflow;
    private readonly PassengerAvailableFlightsWorkflow _availableFlightsWorkflow;
    private readonly PassengerSearchAndBookWorkflow _searchAndBookWorkflow;
    private readonly PassengerFlightGrouper _flightGrouper;
    private readonly PassengerBookingModificationWorkflow _bookingModificationWorkflow;



    public PassengerMenu(
       
        AuthService authService,
        PassengerBookingCancellationWorkflow bookingCancellationWorkflow,
        PassengerBookingViewerWorkflow bookingViewerWorkflow,
        PassengerAvailableFlightsWorkflow availableFlightsWorkflow,
        PassengerSearchAndBookWorkflow searchAndBookWorkflow,
        PassengerFlightGrouper flightGrouper,
        PassengerBookingModificationWorkflow bookingModificationWorkflow)
    {
        
        _authService = authService;
        _bookingCancellationWorkflow = bookingCancellationWorkflow;
        _bookingViewerWorkflow = bookingViewerWorkflow;
        _availableFlightsWorkflow = availableFlightsWorkflow;
        _searchAndBookWorkflow = searchAndBookWorkflow;
        _flightGrouper = flightGrouper;
        _bookingModificationWorkflow = bookingModificationWorkflow;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUi.Header("PASSENGER MENU");

            Console.WriteLine("1. View All Available Flights");
            Console.WriteLine("2. Search & Book Flight");
            Console.WriteLine("3. View My Bookings");
            Console.WriteLine("4. Cancel Booking");
            Console.WriteLine("5. Modify Booking");
            Console.WriteLine("6. Logout");
            Console.WriteLine();

            var choice = ConsoleUi.Prompt("Choose option");

            switch (choice)
            {
                case "1":
                    await ViewAllAvailableFlightsAsync();
                    break;

                case "2":
                    await SearchAndBookFlightAsync();
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

    private async Task ViewAllAvailableFlightsAsync()
    {
        await _availableFlightsWorkflow.ViewAllAvailableFlightsAsync(flights => PrintFlights(flights));
    }

    private async Task SearchAndBookFlightAsync()
    {
        await _searchAndBookWorkflow.SearchAndBookFlightAsync(flights => PrintFlights(flights));
    }

    private async Task ViewMyBookingsAsync()
    {
        await _bookingViewerWorkflow.ViewMyBookingsAsync(bookings => PrintBookings(bookings));
    }

    private async Task CancelBookingAsync()
    {
        await _bookingCancellationWorkflow.CancelBookingAsync(bookings => PrintBookings(bookings));
    }

    private async Task ModifyBookingAsync()
    {
        await _bookingModificationWorkflow.ModifyBookingAsync(
            bookings => PrintBookings(bookings),
            flights => PrintFlights(flights));
    }
    private  void PrintFlights(IReadOnlyList<FlightSearchResult> flights)
    {
        ConsoleUi.Section("Available Flights");

        if (flights.Count == 0)
        {
            ConsoleUi.Info("No flights found.");
            return;
        }

        var groupedFlights = _flightGrouper.GroupFlights(flights);
        const int cardWidth = 56;

        for (var i = 0; i < groupedFlights.Count; i += 2)
        {
            var left = BuildFlightCard(groupedFlights[i], i + 1, cardWidth);

            var right = i + 1 < groupedFlights.Count
                ? BuildFlightCard(groupedFlights[i + 1], i + 2, cardWidth)
                : [];

            var maxLines = Math.Max(left.Count, right.Count);

            for (var line = 0; line < maxLines; line++)
            {
                var leftText = line < left.Count ? left[line] : string.Empty;
                var rightText = line < right.Count ? right[line] : string.Empty;

                Console.Write(leftText.PadRight(cardWidth));

                if (!string.IsNullOrWhiteSpace(rightText))
                    Console.Write("   " + rightText);

                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
    private static List<string> BuildFlightCard(FlightGroup flight, int index, int width)
    {
        var title = $"Option #{index} | {flight.DepartureCountry} to {flight.DestinationCountry}";

        var lines = new List<string>
    {
        title,
        $"Flight No : {flight.FlightNumber} (reference only)",
        $"Airports  : {flight.DepartureAirport} -> {flight.ArrivalAirport}",
        $"Departure : {flight.DepartureDate:yyyy-MM-dd HH:mm}",
        $"Arrival   : {flight.ArrivalDate:yyyy-MM-dd HH:mm}",
        "Classes:"
    };

        foreach (var option in flight.ClassOptions)
        {
            lines.Add($"  {option.TravelClass,-10} | {option.Price,-7} | Seats: {option.AvailableSeats}");
        }

        lines.Add(new string('-', width - 2));

        return lines;
    }

    private static void PrintBookings(IReadOnlyList<BookingResponse> bookings)
    {
        if (bookings.Count == 0)
        {
            ConsoleUi.Info("You have no bookings.");
            return;
        }

        for (var i = 0; i < bookings.Count; i++)
        {
            var booking = bookings[i];

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Booking #{i + 1}");
            Console.ResetColor();

            Console.WriteLine(new string('-', 60));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Route        : ");
            Console.ResetColor();
            Console.WriteLine($"{booking.DepartureCountry} -> {booking.DestinationCountry}");

            Console.WriteLine($"Flight       : {booking.FlightNumber}");
            Console.WriteLine($"Airports     : {booking.DepartureAirport} -> {booking.ArrivalAirport}");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Departure    : ");
            Console.ResetColor();
            Console.WriteLine($"{booking.DepartureDate:yyyy-MM-dd HH:mm}");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Arrival      : ");
            Console.ResetColor();
            Console.WriteLine($"{booking.ArrivalDate:yyyy-MM-dd HH:mm}");

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Class        : ");
            Console.ResetColor();
            Console.WriteLine(booking.TravelClass);

            Console.WriteLine($"Price        : {booking.Price}");

            Console.Write("Status       : ");
            Console.ForegroundColor = booking.Status == BookingStatus.Active
                ? ConsoleColor.Green
                : ConsoleColor.Red;
            Console.WriteLine(booking.Status);
            Console.ResetColor();

            Console.WriteLine(new string('=', 60));
            Console.WriteLine();
        }
    }
}
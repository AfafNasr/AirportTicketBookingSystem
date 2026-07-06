using AirportTicketBookingSystem.Application.DTOs.Bookings;
using AirportTicketBookingSystem.Application.DTOs.Flights;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Input;
using AirportTicketBookingSystem.Domain.Enums;
using AirportTicketBookingSystem.ConsoleApp.Workflows.Passenger;

namespace AirportTicketBookingSystem.ConsoleApp.Menus;

public sealed class PassengerMenu
{
    private readonly FlightService _flightService;
    private readonly BookingService _bookingService;
    private readonly AuthService _authService;
    private readonly PassengerFlightSearchWorkflow _flightSearchWorkflow;
    private readonly PassengerBookingWorkflow _bookingWorkflow;

    private sealed record FlightGroup(
    Guid FlightId,
    string FlightNumber,
    string DepartureCountry,
    string DestinationCountry,
    string DepartureAirport,
    string ArrivalAirport,
    DateTime DepartureDate,
    DateTime ArrivalDate,
    IReadOnlyList<FlightSearchResult> ClassOptions);

    public PassengerMenu(
        FlightService flightService,
        BookingService bookingService,
        AuthService authService,
        PassengerFlightSearchWorkflow flightSearchWorkflow,
        PassengerBookingWorkflow bookingWorkflow)
    {
        _flightService = flightService;
        _bookingService = bookingService;
        _authService = authService;
        _flightSearchWorkflow = flightSearchWorkflow;
        _bookingWorkflow = bookingWorkflow;
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
        ConsoleUi.Header("ALL AVAILABLE FLIGHTS");

        var flights = await _flightService.SearchAvailableFlightsAsync(
            new FlightSearchRequest());

        PrintFlights(flights);

        if (flights.Count == 0)
        {
            ConsoleUi.Pause();
            return;
        }

        var answer = ConsoleUi.Prompt("Do you want to book one of these flights? (Y/N)");

        if (!answer.Equals("Y", StringComparison.OrdinalIgnoreCase))
            return;

        await _bookingWorkflow.BookSelectedFlightAsync(flights);
    }

    private async Task SearchAndBookFlightAsync()
    {
        ConsoleUi.Header("SEARCH & BOOK FLIGHT");

        var flights = await SearchFlightsWithoutPauseAsync();

        if (flights.Count == 0)
        {
            ConsoleUi.Error("No available flights found.");
            ConsoleUi.Pause();
            return;
        }

        await _bookingWorkflow.BookSelectedFlightAsync(flights);
    }

    private async Task<IReadOnlyList<FlightSearchResult>> SearchFlightsWithoutPauseAsync()
    {
        var flights = await _flightSearchWorkflow.SearchFlightsWithoutPauseAsync();

        PrintFlights(flights);

        return flights;
    }

    

    


    private static TravelClass? ChooseTravelClass()
    {
        ConsoleUi.Section("Travel Class");

        Console.WriteLine("0. Any / Skip");
        Console.WriteLine("1. Economy");
        Console.WriteLine("2. Business");
        Console.WriteLine("3. First Class");

        while (true)
        {
            var input = ConsoleUi.Prompt("Choose Travel Class");

            return input switch
            {
                "" or "0" => null,
                "1" => TravelClass.Economy,
                "2" => TravelClass.Business,
                "3" => TravelClass.FirstClass,
                _ => ShowInvalidClassSelection()
            };
        }
    }

    private static TravelClass? ShowInvalidClassSelection()
    {
        ConsoleUi.Error("Invalid class selection.");
        return ChooseTravelClass();
    }

    

    private async Task ViewMyBookingsAsync()
    {
        ConsoleUi.Header("MY BOOKINGS");

        var bookings = await _bookingService.GetMyBookingsAsync();

        PrintBookings(bookings);

        ConsoleUi.Pause();
    }

    private async Task CancelBookingAsync()
    {
        ConsoleUi.Header("CANCEL BOOKING");

        var bookings = (await _bookingService.GetMyBookingsAsync())
       .Where(booking => booking.Status == BookingStatus.Active)
       .ToList();

        PrintBookings(bookings);

        if (bookings.Count == 0)
        {
            ConsoleUi.Pause();
            return;
        }

        var input = ConsoleUi.Prompt("Select booking number to cancel");

        if (!int.TryParse(input, out var selectedIndex) ||
            selectedIndex < 1 ||
            selectedIndex > bookings.Count)
        {
            ConsoleUi.Error("Invalid booking selection.");
            ConsoleUi.Pause();
            return;
        }

        var bookingId = bookings[selectedIndex - 1].BookingId;

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

        var bookings = (await _bookingService.GetMyBookingsAsync())
           .Where(booking => booking.Status == BookingStatus.Active)
           .ToList();

        PrintBookings(bookings);

        if (bookings.Count == 0)
        {
            ConsoleUi.Pause();
            return;
        }

        var bookingInput = ConsoleUi.Prompt("Select booking number to modify");

        if (!int.TryParse(bookingInput, out var selectedBookingIndex) ||
            selectedBookingIndex < 1 ||
            selectedBookingIndex > bookings.Count)
        {
            ConsoleUi.Error("Invalid booking selection.");
            ConsoleUi.Pause();
            return;
        }

        var selectedBooking = bookings[selectedBookingIndex - 1];

        ConsoleUi.Section("Modification Type");
        Console.WriteLine("1. Change Class only");
        Console.WriteLine("2. Change Flight and Class");
        Console.WriteLine("0. Back");

        var choice = ConsoleUi.Prompt("Choose option");

        if (choice == "0")
            return;

        if (choice == "1")
        {
            await ModifyBookingClassOnlyAsync(selectedBooking);
            return;
        }

        if (choice == "2")
        {
            await ModifyBookingFlightAndClassAsync(selectedBooking);
            return;
        }

        ConsoleUi.Error("Invalid option.");
        ConsoleUi.Pause();
    }

    private async Task ModifyBookingClassOnlyAsync(BookingResponse selectedBooking)
    {
        ConsoleUi.Section("Choose New Class");

        var flights = await _flightService.SearchAvailableFlightsAsync(
            new FlightSearchRequest
            {
                DepartureCountry = selectedBooking.DepartureCountry,
                DestinationCountry = selectedBooking.DestinationCountry,
                DepartureDate = selectedBooking.DepartureDate,
                DepartureAirport = selectedBooking.DepartureAirport,
                ArrivalAirport = selectedBooking.ArrivalAirport
            });

        var sameFlightOptions = flights
            .Where(flight => flight.FlightId == selectedBooking.FlightId)
            .OrderBy(flight => flight.TravelClass)
            .ToList();

        if (sameFlightOptions.Count == 0)
        {
            ConsoleUi.Error("No class options found for this flight.");
            ConsoleUi.Pause();
            return;
        }

        for (var i = 0; i < sameFlightOptions.Count; i++)
        {
            var option = sameFlightOptions[i];

            Console.WriteLine(
                $"{i + 1}. {option.TravelClass,-10} | Price: {option.Price,-8} | Seats: {option.AvailableSeats}");
        }

        var input = ConsoleUi.Prompt("Select new class");

        if (!int.TryParse(input, out var selectedClassIndex) ||
            selectedClassIndex < 1 ||
            selectedClassIndex > sameFlightOptions.Count)
        {
            ConsoleUi.Error("Invalid class selection.");
            ConsoleUi.Pause();
            return;
        }

        var selectedOption = sameFlightOptions[selectedClassIndex - 1];

        if (selectedOption.TravelClass == selectedBooking.TravelClass)
        {
            ConsoleUi.Error("You selected the same class. No changes were made.");
            ConsoleUi.Pause();
            return;
        }

        ConsoleUi.Section("Modification Confirmation");
        Console.WriteLine($"Flight       : {selectedBooking.FlightNumber}");
        Console.WriteLine($"Route        : {selectedBooking.DepartureCountry} -> {selectedBooking.DestinationCountry}");
        Console.WriteLine($"Old Class    : {selectedBooking.TravelClass}");
        Console.WriteLine($"New Class    : {selectedOption.TravelClass}");
        Console.WriteLine($"New Price    : {selectedOption.Price}");

        var confirm = ConsoleUi.Prompt("Confirm modification? (Y/N)");

        if (!confirm.Equals("Y", StringComparison.OrdinalIgnoreCase))
        {
            ConsoleUi.Info("Modification cancelled.");
            ConsoleUi.Pause();
            return;
        }

        var result = await _bookingService.ModifyBookingAsync(new ModifyBookingRequest
        {
            BookingId = selectedBooking.BookingId,
            NewFlightId = selectedBooking.FlightId,
            NewTravelClass = selectedOption.TravelClass
        });

        if (result.IsFailure)
            ConsoleUi.Error(result.Error);
        else
            ConsoleUi.Success("Booking class modified successfully.");

        ConsoleUi.Pause();
    }

    private async Task ModifyBookingFlightAndClassAsync(BookingResponse selectedBooking)
    {
        ConsoleUi.Section("Choose New Flight");

        var flights = await SearchFlightsWithoutPauseAsync();

        if (flights.Count == 0)
        {
            ConsoleUi.Error("No flights found.");
            ConsoleUi.Pause();
            return;
        }

        var groupedFlights = GroupFlights(flights);

        var flightInput = ConsoleUi.Prompt("Select new flight option number");

        if (!int.TryParse(flightInput, out var selectedFlightIndex) ||
            selectedFlightIndex < 1 ||
            selectedFlightIndex > groupedFlights.Count)
        {
            ConsoleUi.Error("Invalid flight selection.");
            ConsoleUi.Pause();
            return;
        }

        var selectedFlight = groupedFlights[selectedFlightIndex - 1];

        ConsoleUi.Section("Choose New Class");

        for (var i = 0; i < selectedFlight.ClassOptions.Count; i++)
        {
            var option = selectedFlight.ClassOptions[i];

            Console.WriteLine(
                $"{i + 1}. {option.TravelClass,-10} | Price: {option.Price,-8} | Seats: {option.AvailableSeats}");
        }

        var classInput = ConsoleUi.Prompt("Select new class number");

        if (!int.TryParse(classInput, out var selectedClassIndex) ||
            selectedClassIndex < 1 ||
            selectedClassIndex > selectedFlight.ClassOptions.Count)
        {
            ConsoleUi.Error("Invalid class selection.");
            ConsoleUi.Pause();
            return;
        }

        var selectedOption = selectedFlight.ClassOptions[selectedClassIndex - 1];

        ConsoleUi.Section("Modification Confirmation");
        Console.WriteLine($"Old Flight   : {selectedBooking.FlightNumber}");
        Console.WriteLine($"Old Route    : {selectedBooking.DepartureCountry} -> {selectedBooking.DestinationCountry}");
        Console.WriteLine($"Old Class    : {selectedBooking.TravelClass}");
        Console.WriteLine();
        Console.WriteLine($"New Flight   : {selectedOption.FlightNumber}");
        Console.WriteLine($"New Route    : {selectedOption.DepartureCountry} -> {selectedOption.DestinationCountry}");
        Console.WriteLine($"New Class    : {selectedOption.TravelClass}");
        Console.WriteLine($"New Price    : {selectedOption.Price}");

        var confirm = ConsoleUi.Prompt("Confirm modification? (Y/N)");

        if (!confirm.Equals("Y", StringComparison.OrdinalIgnoreCase))
        {
            ConsoleUi.Info("Modification cancelled.");
            ConsoleUi.Pause();
            return;
        }

        var result = await _bookingService.ModifyBookingAsync(new ModifyBookingRequest
        {
            BookingId = selectedBooking.BookingId,
            NewFlightId = selectedOption.FlightId,
            NewTravelClass = selectedOption.TravelClass
        });

        if (result.IsFailure)
            ConsoleUi.Error(result.Error);
        else
            ConsoleUi.Success("Booking modified successfully.");

        ConsoleUi.Pause();
    }

    private static IReadOnlyList<FlightGroup> GroupFlights(IReadOnlyList<FlightSearchResult> flights)
    {
        return flights
            .GroupBy(flight => new
            {
                flight.FlightId,
                flight.FlightNumber,
                flight.DepartureCountry,
                flight.DestinationCountry,
                flight.DepartureAirport,
                flight.ArrivalAirport,
                flight.DepartureDate,
                flight.ArrivalDate
            })
            .Select(group => new FlightGroup(
                group.Key.FlightId,
                group.Key.FlightNumber,
                group.Key.DepartureCountry,
                group.Key.DestinationCountry,
                group.Key.DepartureAirport,
                group.Key.ArrivalAirport,
                group.Key.DepartureDate,
                group.Key.ArrivalDate,
                group.OrderBy(item => item.TravelClass).ToList()))
            .OrderBy(group => group.DepartureDate)
            .ThenBy(group => group.FlightNumber)
            .ToList();
    }

    private static void PrintFlights(IReadOnlyList<FlightSearchResult> flights)
    {
        ConsoleUi.Section("Available Flights");

        if (flights.Count == 0)
        {
            ConsoleUi.Info("No flights found.");
            return;
        }

        var groupedFlights = GroupFlights(flights);
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
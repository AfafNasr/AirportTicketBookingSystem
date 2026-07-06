using AirportTicketBookingSystem.Application.DTOs.Bookings;
using AirportTicketBookingSystem.Application.DTOs.Flights;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Input;

namespace AirportTicketBookingSystem.ConsoleApp.Workflows.Passenger;

public sealed class PassengerBookingWorkflow
{
    private readonly BookingService _bookingService;

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

    public PassengerBookingWorkflow(BookingService bookingService)
    {
        _bookingService = bookingService;
    }

    public async Task BookSelectedFlightAsync(IReadOnlyList<FlightSearchResult> flights)
    {
        var groupedFlights = GroupFlights(flights);

        var flightInput = ConsoleUi.Prompt("Enter option number to book");

        if (!int.TryParse(flightInput, out var flightIndex) ||
            flightIndex < 1 ||
            flightIndex > groupedFlights.Count)
        {
            ConsoleUi.Error("Invalid flight selection.");
            ConsoleUi.Pause();
            return;
        }

        var selectedFlight = groupedFlights[flightIndex - 1];

        ConsoleUi.Section("Choose Class");

        for (var i = 0; i < selectedFlight.ClassOptions.Count; i++)
        {
            var option = selectedFlight.ClassOptions[i];
            Console.WriteLine($"{i + 1}. {option.TravelClass,-10} | Price: {option.Price} | Seats: {option.AvailableSeats}");
        }

        var classInput = ConsoleUi.Prompt("Select class number");

        if (!int.TryParse(classInput, out var classIndex) ||
            classIndex < 1 ||
            classIndex > selectedFlight.ClassOptions.Count)
        {
            ConsoleUi.Error("Invalid class selection.");
            ConsoleUi.Pause();
            return;
        }

        var selectedOption = selectedFlight.ClassOptions[classIndex - 1];

        ConsoleUi.Section("Booking Confirmation");

        Console.WriteLine($"Flight    : {selectedOption.FlightNumber}");
        Console.WriteLine($"Route     : {selectedOption.DepartureCountry} -> {selectedOption.DestinationCountry}");
        Console.WriteLine($"Departure : {selectedOption.DepartureDate:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"Arrival   : {selectedOption.ArrivalDate:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"Class     : {selectedOption.TravelClass}");
        Console.WriteLine($"Price     : {selectedOption.Price}");
        Console.WriteLine($"Seats     : {selectedOption.AvailableSeats}");

        var confirm = ConsoleUi.Prompt("Confirm booking? (Y/N)");

        if (!confirm.Equals("Y", StringComparison.OrdinalIgnoreCase))
        {
            ConsoleUi.Info("Booking cancelled.");
            ConsoleUi.Pause();
            return;
        }

        var result = await _bookingService.BookFlightAsync(new BookFlightRequest
        {
            FlightId = selectedOption.FlightId,
            TravelClass = selectedOption.TravelClass
        });

        if (result.IsFailure)
            ConsoleUi.Error(result.Error);
        else
            ConsoleUi.Success($"Booking completed successfully. Booking ID: {result.Value!.BookingId}");

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
}
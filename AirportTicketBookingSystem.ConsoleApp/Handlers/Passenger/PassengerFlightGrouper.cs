using AirportTicketBookingSystem.Application.DTOs.Flights;

namespace AirportTicketBookingSystem.ConsoleApp.Workflows.Passenger;

public sealed class PassengerFlightGrouper
{
    public IReadOnlyList<FlightGroup> GroupFlights(IReadOnlyList<FlightSearchResult> flights)
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
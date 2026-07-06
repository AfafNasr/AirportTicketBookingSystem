using AirportTicketBookingSystem.Application.DTOs.Flights;

namespace AirportTicketBookingSystem.ConsoleApp.Workflows.Passenger;

public sealed record FlightGroup(
    Guid FlightId,
    string FlightNumber,
    string DepartureCountry,
    string DestinationCountry,
    string DepartureAirport,
    string ArrivalAirport,
    DateTime DepartureDate,
    DateTime ArrivalDate,
    IReadOnlyList<FlightSearchResult> ClassOptions);
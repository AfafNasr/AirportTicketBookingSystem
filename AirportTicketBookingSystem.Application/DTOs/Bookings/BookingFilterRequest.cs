using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Application.DTOs.Bookings;

public sealed class BookingFilterRequest
{
    public string? FlightNumber { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? DepartureCountry { get; set; }
    public string? DestinationCountry { get; set; }
    public DateTime? DepartureDate { get; set; }
    public string? DepartureAirport { get; set; }
    public string? ArrivalAirport { get; set; }
    public string? PassengerEmail { get; set; }
    public TravelClass? TravelClass { get; set; }
}
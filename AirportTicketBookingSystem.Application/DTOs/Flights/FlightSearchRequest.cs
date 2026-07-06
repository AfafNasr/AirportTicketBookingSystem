

using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Application.DTOs.Flights
{
    public sealed class FlightSearchRequest
    {
        public decimal? MaxPrice { get; set; }
        public string? DepartureCountry { get; set; }
        public string? DestinationCountry { get; set; }
        public DateTime? DepartureDate { get; set; }
        public string? DepartureAirport { get; set; }
        public string? ArrivalAirport { get; set; }
        public TravelClass? TravelClass { get; set; }
    }
}

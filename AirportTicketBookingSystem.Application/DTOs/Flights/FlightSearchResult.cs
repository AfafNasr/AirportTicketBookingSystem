

using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Application.DTOs.Flights
{
    public sealed class FlightSearchResult
    {
        public Guid FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string DepartureCountry { get; set; } = string.Empty;
        public string DestinationCountry { get; set; } = string.Empty;
        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public TravelClass TravelClass { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
    }
}

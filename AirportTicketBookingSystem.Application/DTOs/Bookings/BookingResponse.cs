using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Application.DTOs.Bookings
{
    public sealed class BookingResponse
    {
        public Guid BookingId { get; set; }
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
        public DateTime BookingDate { get; set; }
        public BookingStatus Status { get; set; }
    }
}

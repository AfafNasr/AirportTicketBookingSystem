using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Application.DTOs.Bookings
{
    public sealed class ManagerBookingResult
    {
        public Guid BookingId { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string PassengerEmail { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string DepartureCountry { get; set; } = string.Empty;
        public string DestinationCountry { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
        public TravelClass TravelClass { get; set; }
        public BookingStatus Status { get; set; }
    }
}

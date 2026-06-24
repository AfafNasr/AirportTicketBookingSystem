using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Application.DTOs.Bookings
{
    public sealed class BookFlightRequest
    {
        public Guid FlightId { get; set; }
        public TravelClass TravelClass { get; set; }
    }
}

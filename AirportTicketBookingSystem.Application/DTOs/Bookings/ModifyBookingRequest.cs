using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Application.DTOs.Bookings
{
    public sealed class ModifyBookingRequest
    {
        public Guid BookingId { get; set; }

        public Guid NewFlightId { get; set; }

        public TravelClass NewTravelClass { get; set; }
    }
}

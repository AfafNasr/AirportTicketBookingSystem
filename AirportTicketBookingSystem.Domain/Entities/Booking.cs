using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Domain.Common;
using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Domain.Entities
{
    public sealed class Booking : Entity
    {
        public Guid PassengerUserId { get; private set; }
        public Guid FlightId { get; private set; }
        public TravelClass TravelClass { get; private set; }
        public decimal Price { get; private set; }
        public DateTime BookingDate { get; private set; }
        public BookingStatus Status { get; private set; }

        private Booking()
        {
        }

        public Booking(Guid passengerUserId, Guid flightId, TravelClass travelClass, decimal price)
        {
            PassengerUserId = passengerUserId;
            FlightId = flightId;
            TravelClass = travelClass;
            Price = price;
            BookingDate = DateTime.UtcNow;
            Status = BookingStatus.Active;
        }

        public void Cancel()
        {
            Status = BookingStatus.Cancelled;
        }

        public void Modify(Guid newFlightId, TravelClass newTravelClass, decimal newPrice)
        {
            if (Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("Cancelled bookings cannot be modified.");

            FlightId = newFlightId;
            TravelClass = newTravelClass;
            Price = newPrice;
        }
    }
}

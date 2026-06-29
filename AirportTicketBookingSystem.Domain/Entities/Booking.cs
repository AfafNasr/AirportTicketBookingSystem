using AirportTicketBookingSystem.Domain.Common;
using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Domain.Entities;

public sealed class Booking : Entity
{
    public Guid PassengerUserId { get; set; }
    public Guid FlightId { get; set; }
    public TravelClass TravelClass { get; set; }
    public decimal Price { get; set; }
    public DateTime BookingDate { get; set; }
    public BookingStatus Status { get; set; }

    public Booking()
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
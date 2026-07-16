using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Tests.Entities;

public class BookingTests
{
    [Fact]
    public void Constructor_WhenDataIsValid_CreatesActiveBooking()
    {
        // Arrange
        var passengerUserId = Guid.NewGuid();
        var flightId = Guid.NewGuid();

        // Act
        var booking = new Booking(
            passengerUserId,
            flightId,
            TravelClass.Economy,
            100);

        // Assert
        Assert.Equal(passengerUserId, booking.PassengerUserId);
        Assert.Equal(flightId, booking.FlightId);
        Assert.Equal(TravelClass.Economy, booking.TravelClass);
        Assert.Equal(100, booking.Price);
        Assert.Equal(BookingStatus.Active, booking.Status);
        Assert.NotEqual(default, booking.BookingDate);
    }

    [Fact]
    public void Cancel_WhenBookingIsActive_ChangesStatusToCancelled()
    {
        // Arrange
        var booking = new Booking(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TravelClass.Economy,
            100);

        // Act
        booking.Cancel();

        // Assert
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public void Modify_WhenBookingIsActive_UpdatesBookingDetails()
    {
        // Arrange
        var booking = new Booking(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TravelClass.Economy,
            100);

        var newFlightId = Guid.NewGuid();

        // Act
        booking.Modify(
            newFlightId,
            TravelClass.Business,
            200);

        // Assert
        Assert.Equal(newFlightId, booking.FlightId);
        Assert.Equal(TravelClass.Business, booking.TravelClass);
        Assert.Equal(200, booking.Price);
    }
    [Fact]
    public void Modify_WhenBookingIsCancelled_ThrowsInvalidOperationException()
    {
        // Arrange
        var booking = new Booking(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TravelClass.Economy,
            100);

        booking.Cancel();

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
            booking.Modify(
                Guid.NewGuid(),
                TravelClass.Business,
                200));

        // Assert
        Assert.Equal(
            "Cancelled bookings cannot be modified.",
            exception.Message);
    }

}

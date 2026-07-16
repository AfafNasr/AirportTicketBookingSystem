using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Application.Abstractions.Services;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Domain.Enums;
using Moq;

namespace AirportTicketBookingSystem.Tests.Services.BookingServiceTests;

public class CancelBookingAsyncTests
{
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IFlightRepository> _flightRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrentUserSession> _currentUserSessionMock;

    private readonly BookingService _bookingService;

    public CancelBookingAsyncTests()
    {
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _flightRepositoryMock = new Mock<IFlightRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _currentUserSessionMock = new Mock<ICurrentUserSession>();

        _bookingService = new BookingService(
            _bookingRepositoryMock.Object,
            _flightRepositoryMock.Object,
            _userRepositoryMock.Object,
            _currentUserSessionMock.Object);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenUserIsNotAuthenticated_ReturnsFailure()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(false);

        // Act
        var result = await _bookingService.CancelBookingAsync(bookingId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "You must login before cancelling a booking.",
            result.Error);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenUserIsNotPassenger_ReturnsFailure()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        _currentUserSessionMock
           .Setup(x => x.IsAuthenticated)
           .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid());

        _currentUserSessionMock
            .Setup(x => x.Role)
            .Returns(UserRole.Manager);

        // Act
        var result = await _bookingService.CancelBookingAsync(bookingId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Only passengers can cancel their bookings.",
            result.Error);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenBookingwasNotFound_ReturnsFailure()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        _currentUserSessionMock
           .Setup(x => x.IsAuthenticated)
           .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid());

        _currentUserSessionMock
            .Setup(x => x.Role)
            .Returns(UserRole.Passenger);


        _bookingRepositoryMock
           .Setup(x => x.GetByIdAsync(bookingId))
           .ReturnsAsync((Booking?)null);


        // Act
        var result = await _bookingService.CancelBookingAsync(bookingId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Booking was not found.",
            result.Error);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenUserNotAllowedToCancel_ReturnsFailure()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var bookingOwnerId = Guid.NewGuid(); 

        var booking = new Booking(
            bookingOwnerId,
            Guid.NewGuid(),
            TravelClass.Economy,
            100);

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.UserId)
            .Returns(currentUserId);

        _currentUserSessionMock
            .Setup(x => x.Role)
            .Returns(UserRole.Passenger);

        _bookingRepositoryMock
            .Setup(x => x.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);


        // Act
        var result = await _bookingService.CancelBookingAsync(bookingId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "You are not allowed to cancel this booking.",
            result.Error);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenBookingIsAlreadyCanceled_ReturnsFailure()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        

        var booking = new Booking(
            currentUserId,
            Guid.NewGuid(),
            TravelClass.Economy,
            100);

        booking.Cancel();

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.UserId)
            .Returns(currentUserId);

        _currentUserSessionMock
            .Setup(x => x.Role)
            .Returns(UserRole.Passenger);

        _bookingRepositoryMock
            .Setup(x => x.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);


        // Act
        var result = await _bookingService.CancelBookingAsync(bookingId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Booking is already cancelled.",
            result.Error);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenFlightwasNotFound_ReturnsFailure()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var flightId = Guid.NewGuid();

        var booking = new Booking(
        currentUserId,
        flightId,
        TravelClass.Economy,
        100);

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.UserId)
            .Returns(currentUserId);

        _currentUserSessionMock
            .Setup(x => x.Role)
            .Returns(UserRole.Passenger);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(flightId))
            .ReturnsAsync((Flight?)null);

        _bookingRepositoryMock
            .Setup(x => x.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        // Act
        var result = await _bookingService.CancelBookingAsync(bookingId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Flight was not found.",
            result.Error);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenRequestIsValid_CancelsBookingAndReleasesSeat()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        var departureDate = DateTime.Now.AddDays(2);
        var arrivalDate = departureDate.AddHours(3);

        var flight = new Flight(
            "RJ101",
            "Jordan",
            "UAE",
            "Queen Alia",
            "Dubai",
            departureDate,
            arrivalDate,
            100,
            200,
            300,
            9,
            10,
            10);

        var booking = new Booking(
            currentUserId,
            flight.Id,
            TravelClass.Economy,
            100);

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.UserId)
            .Returns(currentUserId);

        _currentUserSessionMock
            .Setup(x => x.Role)
            .Returns(UserRole.Passenger);

        _bookingRepositoryMock
            .Setup(x => x.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(flight.Id))
            .ReturnsAsync(flight);

        // Act
        var result = await _bookingService.CancelBookingAsync(bookingId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal(10, flight.EconomySeats);

    }

}

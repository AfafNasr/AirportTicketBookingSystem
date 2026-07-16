using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Application.Abstractions.Services;
using AirportTicketBookingSystem.Application.DTOs.Bookings;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Domain.Enums;
using Moq;

namespace AirportTicketBookingSystem.Tests.Services.BookingServiceTests;

public class BookFlightAsyncTests
{
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IFlightRepository> _flightRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrentUserSession> _currentUserSessionMock;

    private readonly BookingService _bookingService;

    public BookFlightAsyncTests()
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
    public async Task BookFlightAsync_WhenUserIsNotAuthenticated_ReturnsFailure()
    {
        // Arrange
        var request = new BookFlightRequest
        {
            FlightId = Guid.NewGuid(),
            TravelClass = TravelClass.Economy
        };

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(false);

        // Act
        var result = await _bookingService.BookFlightAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "You must login before booking a flight.",
            result.Error);
    }

    [Fact]
    public async Task BookFlightAsync_WhenUserIsNotPassenger_ReturnsFailure()
    {
        // Arrange
        var request = new BookFlightRequest
        {
            FlightId = Guid.NewGuid(),
            TravelClass = TravelClass.Economy
        };

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
        var result = await _bookingService.BookFlightAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Only passengers can book flights.",
            result.Error);
    }


        [Fact]
    public async Task BookFlightAsync_WhenFlightIsNotFound_ReturnsFailure()
    {
        // Arrange
        var request = new BookFlightRequest
        {
            FlightId = Guid.NewGuid(),
            TravelClass = TravelClass.Economy
        };
        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid());

        _currentUserSessionMock
            .Setup(x => x.Role)
            .Returns(UserRole.Passenger);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(request.FlightId))
            .ReturnsAsync((Flight?)null);

        // Act
        var result = await _bookingService.BookFlightAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Flight was not found.",
            result.Error);
    }

    [Fact]
    public async Task BookFlightAsync_WhenFlightIsAlreadyBooked_ReturnsFailure()
    {
        // Arrange
        var passengerId = Guid.NewGuid();

        var request = new BookFlightRequest
        {
            FlightId = Guid.NewGuid(),
            TravelClass = TravelClass.Economy
        };

        var flight = new Flight(
            "RJ101",
            "Jordan",
            "UAE",
            "Queen Alia",
            "Dubai",
            DateTime.Now.AddDays(2),
            DateTime.Now.AddDays(2).AddHours(3),
            100,
            200,
            300,
            10,
            10,
            10);

        var booking = new Booking(
            passengerId,
            request.FlightId,
            TravelClass.Economy,
            100);

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.UserId)
            .Returns(passengerId);

        _currentUserSessionMock
            .Setup(x => x.Role)
            .Returns(UserRole.Passenger);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(request.FlightId))
            .ReturnsAsync(flight);

        _bookingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Booking>
            {
            booking
            });

        // Act
        var result = await _bookingService.BookFlightAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "You already have an active booking for this flight.",
            result.Error);
    }

    [Fact]
    public async Task BookFlightAsync_WhenFlightHasDeparted_ReturnsFailure()
    {
        // Arrange
        var passengerId = Guid.NewGuid();
        var now = DateTime.Now;

        var request = new BookFlightRequest
        {
            FlightId = Guid.NewGuid(),
            TravelClass = TravelClass.Economy
        };

        var flight = new Flight(
            "RJ101",
            "Jordan",
            "UAE",
            "Queen Alia",
            "Dubai",
            now.AddDays(-2),
            now.AddDays(-2).AddHours(3),
            100,
            200,
            300,
            10,
            10,
            10);

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.UserId)
            .Returns(passengerId);

        _currentUserSessionMock
            .Setup(x => x.Role)
            .Returns(UserRole.Passenger);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(request.FlightId))
            .ReturnsAsync(flight);

        _bookingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Booking>());

        // Act
        var result = await _bookingService.BookFlightAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Cannot book a departed flight.",
            result.Error);
    }

    [Theory]
    [InlineData(TravelClass.Economy, 0, 10, 10)]
    [InlineData(TravelClass.Business, 10, 0, 10)]
    [InlineData(TravelClass.FirstClass, 10, 10, 0)]
    public async Task BookFlightAsync_WhenNoAvailableSeats_ReturnsFailure(
    TravelClass travelClass,
    int economySeats,
    int businessSeats,
    int firstClassSeats)
    {
        // Arrange
        var passengerId = Guid.NewGuid();

        var request = new BookFlightRequest
        {
            FlightId = Guid.NewGuid(),
            TravelClass = travelClass
        };

        var now = DateTime.Now;

        var flight = new Flight(
            "RJ101",
            "Jordan",
            "UAE",
            "Queen Alia",
            "Dubai",
            now.AddDays(2),
            now.AddDays(2).AddHours(3),
            100,
            200,
            300,
            economySeats,
            businessSeats,
            firstClassSeats);

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.UserId)
            .Returns(passengerId);

        _currentUserSessionMock
            .Setup(x => x.Role)
            .Returns(UserRole.Passenger);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(request.FlightId))
            .ReturnsAsync(flight);

        _bookingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Booking>());

        // Act
        var result = await _bookingService.BookFlightAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "No available seats for the selected class.",
            result.Error);
    }

    [Fact]
    public async Task BookFlightAsync_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var passengerId = Guid.NewGuid();
        var now = DateTime.Now;

        var request = new BookFlightRequest
        {
            FlightId = Guid.NewGuid(),
            TravelClass = TravelClass.Economy
        };

        var flight = new Flight(
            "RJ101",
            "Jordan",
            "UAE",
            "Queen Alia",
            "Dubai",
            now.AddDays(2),
            now.AddDays(2).AddHours(3),
            100,
            200,
            300,
            10,
            10,
            10);

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.UserId)
            .Returns(passengerId);

        _currentUserSessionMock
            .Setup(x => x.Role)
            .Returns(UserRole.Passenger);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(request.FlightId))
            .ReturnsAsync(flight);

        _bookingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Booking>());

        // Act
        var result = await _bookingService.BookFlightAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.Price);
        Assert.Equal(9, flight.EconomySeats);
    }


}



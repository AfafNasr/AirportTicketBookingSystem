using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Application.Abstractions.Services;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Domain.Enums;
using Moq;

namespace AirportTicketBookingSystem.Tests.Services.BookingServiceTests;

public  class GetMyBookingsAsyncTests
{
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IFlightRepository> _flightRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrentUserSession> _currentUserSessionMock;

    private readonly BookingService _bookingService;

    public GetMyBookingsAsyncTests()
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
    public async Task GetMyBookingsAsync_WhenUserIsNotAuthenticated_ReturnsEmptyList()
    {
        // Arrange
        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(false);

        // Act
        var result = await _bookingService.GetMyBookingsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMyBookingsAsync_WhenFlightIsMissing_ReturnsUnknownFlightNumber()
    {
        // Arrange
        var passengerId = Guid.NewGuid();
        var missingFlightId = Guid.NewGuid();

        var booking = new Booking(
            passengerId,
            missingFlightId,
            TravelClass.Economy,
            100);

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.UserId)
            .Returns(passengerId);

        _bookingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Booking>
            {
            booking
            });

        _flightRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Flight>());

        // Act
        var result = await _bookingService.GetMyBookingsAsync();

        // Assert
        Assert.Equal("Unknown", result[0].FlightNumber);
    }

    [Fact]
    public async Task GetMyBookingsAsync_WhenUserHasBookings_ReturnsOnlyHisBookings()
    {
        // Arrange
        var currentPassengerId = Guid.NewGuid();
        var anotherPassengerId = Guid.NewGuid();

        var firstFlight = new Flight(
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

        var secondFlight = new Flight(
            "RJ202",
            "Jordan",
            "Egypt",
            "Queen Alia",
            "Cairo",
            DateTime.Now.AddDays(3),
            DateTime.Now.AddDays(3).AddHours(2),
            120,
            220,
            320,
            10,
            10,
            10);

        var currentUserBooking1 = new Booking(
            currentPassengerId,
            firstFlight.Id,
            TravelClass.Economy,
            100);

        var currentUserBooking2 = new Booking(
            currentPassengerId,
            secondFlight.Id,
            TravelClass.Business,
            220);

        var anotherUserBooking = new Booking(
            anotherPassengerId,
            firstFlight.Id,
            TravelClass.FirstClass,
            300);

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.UserId)
            .Returns(currentPassengerId);

        _bookingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Booking>
            {
            currentUserBooking1,
            currentUserBooking2,
            anotherUserBooking
            });

        _flightRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Flight>
            {
            firstFlight,
            secondFlight
            });

        // Act
        var result = await _bookingService.GetMyBookingsAsync();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.DoesNotContain(
            result,
            booking => booking.BookingId == anotherUserBooking.Id);
    }

    [Fact]
    public async Task GetMyBookingsAsync_ReturnsBookingsOrderedByNewest()
    {
        // Arrange
        var passengerId = Guid.NewGuid();

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

        var oldBooking = new Booking(
            passengerId,
            flight.Id,
            TravelClass.Economy,
            100)
        {
            BookingDate = new DateTime(2026, 1, 1)
        };

        var newBooking = new Booking(
            passengerId,
            flight.Id,
            TravelClass.Business,
            200)
        {
            BookingDate = new DateTime(2026, 2, 1)
        };

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.UserId)
            .Returns(passengerId);

        _bookingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Booking>
            {
            oldBooking,
            newBooking
            });

        _flightRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Flight>
            {
            flight
            });

        // Act
        var result = await _bookingService.GetMyBookingsAsync();

        // Assert
        Assert.Equal(newBooking.Id, result[0].BookingId);
        Assert.Equal(oldBooking.Id, result[1].BookingId);
    }

}

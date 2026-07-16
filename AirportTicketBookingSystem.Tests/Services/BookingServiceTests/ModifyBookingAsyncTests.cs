using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Application.Abstractions.Services;
using AirportTicketBookingSystem.Application.DTOs.Bookings;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Domain.Enums;
using Moq;

namespace AirportTicketBookingSystem.Tests.Services.BookingServiceTests;

public class ModifyBookingAsyncTests
{
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IFlightRepository> _flightRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrentUserSession> _currentUserSessionMock;

    private readonly BookingService _bookingService;

    public ModifyBookingAsyncTests()
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
    public async Task ModifyBookingAsync_WhenUserIsNotAuthenticated_ReturnsFailure()
    {
        // Arrange
        var request = new ModifyBookingRequest
        {
            BookingId = Guid.NewGuid(),
            NewFlightId = Guid.NewGuid()
        };

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(false);

        // Act
        var result = await _bookingService.ModifyBookingAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "You must login before modifying a booking.",
            result.Error);
    }

    [Fact]
    public async Task ModifyBookingAsync_WhenUserIsNotPassenger_ReturnsFailure()
    {
        // Arrange
        var request = new ModifyBookingRequest
        {
            BookingId = Guid.NewGuid(),
            NewFlightId = Guid.NewGuid()
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
        var result = await _bookingService.ModifyBookingAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Only passengers can modify their bookings.",
            result.Error);
    }
    [Fact]
    public async Task ModifyBookingAsync_WhenBookingWasNotFound_ReturnsFailure()
    {
        // Arrange
        var request = new ModifyBookingRequest
        {
            BookingId = Guid.NewGuid(),
            NewFlightId = Guid.NewGuid()
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

        _bookingRepositoryMock
            .Setup(x => x.GetByIdAsync(request.BookingId))
            .ReturnsAsync((Booking?)null);

        // Act
        var result = await _bookingService.ModifyBookingAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Booking was not found.",
            result.Error);
    }

    [Fact]
    public async Task ModifyBookingAsync_WhenUserWasNotAllowedToModify_ReturnsFailure()
    {
        // Arrange
        var bookingOwnerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        var request = new ModifyBookingRequest
        {
            BookingId = Guid.NewGuid(),
            NewFlightId = Guid.NewGuid(),
            NewTravelClass = TravelClass.Business

        };

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
            .Setup(x => x.GetByIdAsync(request.BookingId))
            .ReturnsAsync(booking);

        // Act
        var result = await _bookingService.ModifyBookingAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "You are not allowed to modify this booking.",
            result.Error);
    }

    [Fact]
    public async Task ModifyBookingAsync_WhenBookingStatusWasCancelled_ReturnsFailure()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();

        var request = new ModifyBookingRequest
        {
            BookingId = Guid.NewGuid(),
            NewFlightId = Guid.NewGuid(),
            NewTravelClass = TravelClass.Business

        };

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
            .Setup(x => x.GetByIdAsync(request.BookingId))
            .ReturnsAsync(booking);

        // Act
        var result = await _bookingService.ModifyBookingAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Cancelled bookings cannot be modified.",
            result.Error);
    }

    [Fact]
    public async Task ModifyBookingAsync_WhenOriginalFlightWasNotFound_ReturnsFailure()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var flightId = Guid.NewGuid();

        var request = new ModifyBookingRequest
        {
            BookingId = Guid.NewGuid(),
            NewFlightId = Guid.NewGuid(),
            NewTravelClass = TravelClass.Business

        };

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

        _bookingRepositoryMock
            .Setup(x => x.GetByIdAsync(request.BookingId))
            .ReturnsAsync(booking);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(flightId))
            .ReturnsAsync((Flight?)null);

        // Act
        var result = await _bookingService.ModifyBookingAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Original flight was not found.",
            result.Error);
    }

    [Fact]
    public async Task ModifyBookingAsync_WhenNewFlightWasNotFound_ReturnsFailure()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var originalFlightId = Guid.NewGuid();

        var request = new ModifyBookingRequest
        {
            BookingId = Guid.NewGuid(),
            NewFlightId = Guid.NewGuid(),
            NewTravelClass = TravelClass.Business
        };

        var booking = new Booking(
            currentUserId,
            originalFlightId,
            TravelClass.Economy,
            100);

        var originalFlight = new Flight(
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
            .Setup(x => x.GetByIdAsync(request.BookingId))
            .ReturnsAsync(booking);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(originalFlightId))
            .ReturnsAsync(originalFlight);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(request.NewFlightId))
            .ReturnsAsync((Flight?)null);

        // Act
        var result = await _bookingService.ModifyBookingAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "New flight was not found.",
            result.Error);
    }

    [Fact]
    public async Task ModifyBookingAsync_WhenNewFlightHasDeparted_ReturnsFailure()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var originalFlightId = Guid.NewGuid();
        var newFlightId = Guid.NewGuid();

        var request = new ModifyBookingRequest
        {
            BookingId = Guid.NewGuid(),
            NewFlightId = newFlightId,
            NewTravelClass = TravelClass.Business
        };

        var booking = new Booking(
            currentUserId,
            originalFlightId,
            TravelClass.Economy,
            100);

        var originalFlight = new Flight(
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

        var departedFlight = new Flight(
            "RJ202",
            "Jordan",
            "Egypt",
            "Queen Alia",
            "Cairo",
            DateTime.Now.AddDays(-2),
            DateTime.Now.AddDays(-2).AddHours(3),
            120,
            220,
            320,
            10,
            10,
            10);

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
            .Setup(x => x.GetByIdAsync(request.BookingId))
            .ReturnsAsync(booking);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(originalFlightId))
            .ReturnsAsync(originalFlight);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(newFlightId))
            .ReturnsAsync(departedFlight);

        // Act
        var result = await _bookingService.ModifyBookingAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Cannot change booking to a departed flight.",
            result.Error);
    }

    [Fact]
    public async Task ModifyBookingAsync_WhenSameFlightAndSameClass_ReturnsFailure()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();

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
            currentUserId,
            flight.Id,
            TravelClass.Economy,
            100);

        var request = new ModifyBookingRequest
        {
            BookingId = Guid.NewGuid(),
            NewFlightId = flight.Id,
            NewTravelClass = TravelClass.Economy
        };

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
            .Setup(x => x.GetByIdAsync(request.BookingId))
            .ReturnsAsync(booking);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(flight.Id))
            .ReturnsAsync(flight);

        // Act
        var result = await _bookingService.ModifyBookingAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "No changes were made.",
            result.Error);
    }

    [Fact]
    public async Task ModifyBookingAsync_WhenSameFlightHasNoSeatsInNewClass_ReturnsFailure()
    {
        // Arrange
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
            0,  
            10);

        var booking = new Booking(
            currentUserId,
            flight.Id,
            TravelClass.Economy,
            100);

        var request = new ModifyBookingRequest
        {
            BookingId = Guid.NewGuid(),
            NewFlightId = flight.Id,
            NewTravelClass = TravelClass.Business
        };

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
            .Setup(x => x.GetByIdAsync(request.BookingId))
            .ReturnsAsync(booking);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(flight.Id))
            .ReturnsAsync(flight);

        // Act
        var result = await _bookingService.ModifyBookingAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "No available seats for the selected class.",
            result.Error);

        Assert.Equal(9, flight.EconomySeats);
        Assert.Equal(0, flight.BusinessSeats);
    }

    [Fact]
    public async Task ModifyBookingAsync_WhenSameFlightHasAvailableSeat_ChangesTravelClassSuccessfully()
    {
        // Arrange
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
            5,  
            10);

        var booking = new Booking(
            currentUserId,
            flight.Id,
            TravelClass.Economy,
            100);

        var request = new ModifyBookingRequest
        {
            BookingId = Guid.NewGuid(),
            NewFlightId = flight.Id,
            NewTravelClass = TravelClass.Business
        };

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
            .Setup(x => x.GetByIdAsync(request.BookingId))
            .ReturnsAsync(booking);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(flight.Id))
            .ReturnsAsync(flight);

        // Act
        var result = await _bookingService.ModifyBookingAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(10, flight.EconomySeats);
        Assert.Equal(4, flight.BusinessSeats);

        Assert.Equal(TravelClass.Business, booking.TravelClass);
        Assert.Equal(200, booking.Price);

    }

    [Fact]
    public async Task ModifyBookingAsync_WhenNewFlightHasNoAvailableSeat_ReturnsFailure()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();

        var oldDepartureDate = DateTime.Now.AddDays(2);
        var newDepartureDate = DateTime.Now.AddDays(3);

        var oldFlight = new Flight(
            "RJ101",
            "Jordan",
            "UAE",
            "Queen Alia",
            "Dubai",
            oldDepartureDate,
            oldDepartureDate.AddHours(3),
            100,
            200,
            300,
            9,
            10,
            10);

        var newFlight = new Flight(
            "RJ202",
            "Jordan",
            "Egypt",
            "Queen Alia",
            "Cairo",
            newDepartureDate,
            newDepartureDate.AddHours(2),
            120,
            220,
            320,
            10,
            0,
            10);

        var booking = new Booking(
            currentUserId,
            oldFlight.Id,
            TravelClass.Economy,
            100);

        var request = new ModifyBookingRequest
        {
            BookingId = Guid.NewGuid(),
            NewFlightId = newFlight.Id,
            NewTravelClass = TravelClass.Business
        };

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
            .Setup(x => x.GetByIdAsync(request.BookingId))
            .ReturnsAsync(booking);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(oldFlight.Id))
            .ReturnsAsync(oldFlight);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(newFlight.Id))
            .ReturnsAsync(newFlight);

        // Act
        var result = await _bookingService.ModifyBookingAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "No available seats for the selected class.",
            result.Error);

        Assert.Equal(9, oldFlight.EconomySeats);
        Assert.Equal(0, newFlight.BusinessSeats);
    }

    [Fact]
    public async Task ModifyBookingAsync_WhenNewFlightHasAvailableSeat_ChangesFlightSuccessfully()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();

        var oldDepartureDate = DateTime.Now.AddDays(2);
        var newDepartureDate = DateTime.Now.AddDays(3);

        var oldFlight = new Flight(
            "RJ101",
            "Jordan",
            "UAE",
            "Queen Alia",
            "Dubai",
            oldDepartureDate,
            oldDepartureDate.AddHours(3),
            100,
            200,
            300,
            9,
            10,
            10);

        var newFlight = new Flight(
            "RJ202",
            "Jordan",
            "Egypt",
            "Queen Alia",
            "Cairo",
            newDepartureDate,
            newDepartureDate.AddHours(2),
            120,
            220,
            320,
            10,
            5,
            10);

        var booking = new Booking(
            currentUserId,
            oldFlight.Id,
            TravelClass.Economy,
            100);

        var request = new ModifyBookingRequest
        {
            BookingId = Guid.NewGuid(),
            NewFlightId = newFlight.Id,
            NewTravelClass = TravelClass.Business
        };

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
            .Setup(x => x.GetByIdAsync(request.BookingId))
            .ReturnsAsync(booking);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(oldFlight.Id))
            .ReturnsAsync(oldFlight);

        _flightRepositoryMock
            .Setup(x => x.GetByIdAsync(newFlight.Id))
            .ReturnsAsync(newFlight);

        // Act
        var result = await _bookingService.ModifyBookingAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(10, oldFlight.EconomySeats);
        Assert.Equal(4, newFlight.BusinessSeats);

        Assert.Equal(newFlight.Id, booking.FlightId);
        Assert.Equal(TravelClass.Business, booking.TravelClass);
        Assert.Equal(220, booking.Price);
    }
}

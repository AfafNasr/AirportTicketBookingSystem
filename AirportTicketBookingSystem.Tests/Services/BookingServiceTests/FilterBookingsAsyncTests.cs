using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Application.Abstractions.Services;
using AirportTicketBookingSystem.Application.DTOs.Bookings;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Domain.Enums;
using Moq;

namespace AirportTicketBookingSystem.Tests.Services.BookingServiceTests;

public class FilterBookingsAsyncTests
{
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IFlightRepository> _flightRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrentUserSession> _currentUserSessionMock;

    private readonly BookingService _bookingService;

    public FilterBookingsAsyncTests()
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
    public async Task FilterBookingsAsync_WhenUserIsNotAuthenticated_ReturnsEmptyList()
    {
        // Arrange
        var request = new BookingFilterRequest();

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(false);

        // Act
        var result = await _bookingService.FilterBookingsAsync(request);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task FilterBookingsAsync_WhenUserIsNotManager_ReturnsEmptyList()
    {
        // Arrange
        var request = new BookingFilterRequest();

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.Role)
            .Returns(UserRole.Passenger);

        // Act
        var result = await _bookingService.FilterBookingsAsync(request);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task FilterBookingsAsync_WhenNoFiltersProvided_ReturnsAllBookings()
    {
        // Arrange
        var firstUser = new User(
            "Alice Passenger",
            "alice@example.com",
            "hashed-password",
            UserRole.Passenger);

        var secondUser = new User(
            "Bob Passenger",
            "bob@example.com",
            "hashed-password",
            UserRole.Passenger);

        var firstDepartureDate = new DateTime(2030, 1, 10, 10, 0, 0);
        var secondDepartureDate = new DateTime(2030, 1, 12, 12, 0, 0);

        var firstFlight = new Flight(
            "RJ101",
            "Jordan",
            "UAE",
            "Queen Alia",
            "Dubai",
            firstDepartureDate,
            firstDepartureDate.AddHours(3),
            100,
            200,
            300,
            10,
            10,
            10);

        var secondFlight = new Flight(
            "MS202",
            "Egypt",
            "France",
            "Cairo",
            "Charles de Gaulle",
            secondDepartureDate,
            secondDepartureDate.AddHours(4),
            150,
            250,
            350,
            10,
            10,
            10);

        var firstBooking = new Booking(
            firstUser.Id,
            firstFlight.Id,
            TravelClass.Economy,
            100);

        var secondBooking = new Booking(
            secondUser.Id,
            secondFlight.Id,
            TravelClass.Business,
            250);

        var request = new BookingFilterRequest();

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.Role)
            .Returns(UserRole.Manager);

        _bookingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Booking>
            {
            firstBooking,
            secondBooking
            });

        _flightRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Flight>
            {
            firstFlight,
            secondFlight
            });

        _userRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
            firstUser,
            secondUser
            });

        // Act
        var result = await _bookingService.FilterBookingsAsync(request);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task FilterBookingsAsync_WhenFiltersAreProvided_ReturnsMatchingBooking()
    {
        // Arrange
        var matchingUser = new User(
            "Alice Passenger",
            "alice@example.com",
            "hashed-password",
            UserRole.Passenger);

        var otherUser = new User(
            "Bob Passenger",
            "bob@example.com",
            "hashed-password",
            UserRole.Passenger);

        var matchingDepartureDate = new DateTime(2030, 1, 10, 10, 0, 0);
        var otherDepartureDate = new DateTime(2030, 1, 12, 12, 0, 0);

        var matchingFlight = new Flight(
            "RJ101",
            "Jordan",
            "UAE",
            "Queen Alia",
            "Dubai",
            matchingDepartureDate,
            matchingDepartureDate.AddHours(3),
            100,
            200,
            300,
            10,
            10,
            10);

        var otherFlight = new Flight(
            "MS202",
            "Egypt",
            "France",
            "Cairo",
            "Charles de Gaulle",
            otherDepartureDate,
            otherDepartureDate.AddHours(4),
            150,
            250,
            350,
            10,
            10,
            10);

        var matchingBooking = new Booking(
            matchingUser.Id,
            matchingFlight.Id,
            TravelClass.Economy,
            100);

        var otherBooking = new Booking(
            otherUser.Id,
            otherFlight.Id,
            TravelClass.Business,
            250);

        var request = new BookingFilterRequest
        {
            FlightNumber = "  rj101  ",
            MaxPrice = 150,
            DepartureCountry = "jordan",
            DestinationCountry = "uae",
            DepartureDate = matchingDepartureDate,
            DepartureAirport = "queen alia",
            ArrivalAirport = "dubai",
            PassengerEmail = "alice@example.com",
            TravelClass = TravelClass.Economy
        };

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.Role)
            .Returns(UserRole.Manager);

        _bookingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Booking>
            {
            matchingBooking,
            otherBooking
            });

        _flightRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Flight>
            {
            matchingFlight,
            otherFlight
            });

        _userRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
            matchingUser,
            otherUser
            });

        // Act
        var result = await _bookingService.FilterBookingsAsync(request);

        // Assert
        var returnedBooking = Assert.Single(result);

        Assert.Equal(matchingBooking.Id, returnedBooking.BookingId);
        Assert.Equal("RJ101", returnedBooking.FlightNumber);
        Assert.Equal("alice@example.com", returnedBooking.PassengerEmail);
        Assert.Equal(TravelClass.Economy, returnedBooking.TravelClass);
    }

    [Fact]
    public async Task FilterBookingsAsync_ReturnsBookingsOrderedByDateThenFlightNumber()
    {
        // Arrange
        var passenger = new User(
            "Alice Passenger",
            "alice@example.com",
            "hashed-password",
            UserRole.Passenger);

        var sameDepartureDate = new DateTime(2030, 1, 10, 10, 0, 0);
        var laterDepartureDate = new DateTime(2030, 1, 12, 10, 0, 0);

        var flightRJ200 = new Flight(
            "RJ200",
            "Jordan",
            "UAE",
            "Queen Alia",
            "Dubai",
            sameDepartureDate,
            sameDepartureDate.AddHours(3),
            100,
            200,
            300,
            10,
            10,
            10);

        var flightRJ100 = new Flight(
            "RJ100",
            "Jordan",
            "Egypt",
            "Queen Alia",
            "Cairo",
            sameDepartureDate,
            sameDepartureDate.AddHours(2),
            120,
            220,
            320,
            10,
            10,
            10);

        var laterFlight = new Flight(
            "RJ050",
            "Jordan",
            "France",
            "Queen Alia",
            "Charles de Gaulle",
            laterDepartureDate,
            laterDepartureDate.AddHours(4),
            150,
            250,
            350,
            10,
            10,
            10);

        var bookingRJ200 = new Booking(
            passenger.Id,
            flightRJ200.Id,
            TravelClass.Economy,
            100);

        var bookingRJ100 = new Booking(
            passenger.Id,
            flightRJ100.Id,
            TravelClass.Economy,
            120);

        var laterBooking = new Booking(
            passenger.Id,
            laterFlight.Id,
            TravelClass.Economy,
            150);

        var request = new BookingFilterRequest();

        _currentUserSessionMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserSessionMock
            .Setup(x => x.Role)
            .Returns(UserRole.Manager);

        _bookingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Booking>
            {
            laterBooking,
            bookingRJ200,
            bookingRJ100
            });

        _flightRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Flight>
            {
            laterFlight,
            flightRJ200,
            flightRJ100
            });

        _userRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
            passenger
            });

        // Act
        var result = await _bookingService.FilterBookingsAsync(request);

        // Assert
        Assert.Equal("RJ100", result[0].FlightNumber);
        Assert.Equal("RJ200", result[1].FlightNumber);
        Assert.Equal("RJ050", result[2].FlightNumber);
    }

}

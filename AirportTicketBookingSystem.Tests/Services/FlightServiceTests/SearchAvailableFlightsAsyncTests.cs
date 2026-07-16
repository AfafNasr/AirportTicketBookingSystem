using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Application.DTOs.Flights;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Domain.Enums;
using Moq;

namespace AirportTicketBookingSystem.Tests.Services.FlightServiceTests;

public class SearchAvailableFlightsAsyncTests
{
    private readonly Mock<IFlightRepository> _flightRepositoryMock;
    private readonly FlightService _flightService;

    public SearchAvailableFlightsAsyncTests()
    {
        _flightRepositoryMock = new Mock<IFlightRepository>();

        _flightService = new FlightService(
            _flightRepositoryMock.Object);
    }
    [Fact]
    public async Task SearchAvailableFlightsAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        // Act + Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _flightService.SearchAvailableFlightsAsync(null!));
    }

    [Fact]
    public async Task SearchAvailableFlightsAsync_WhenFlightHasDeparted_DoesNotReturnFlight()
    {
        // Arrange
        var departureDate = DateTime.Now.AddDays(-2);
        var arrivalDate = departureDate.AddHours(3);

        var departedFlight = new Flight(
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
            10,
            10,
            10);

        _flightRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Flight>
            {
            departedFlight
            });

        var request = new FlightSearchRequest();

        // Act
        var result =
            await _flightService.SearchAvailableFlightsAsync(request);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAvailableFlightsAsync_WhenNoFiltersProvided_ReturnsAllAvailableClasses()
    {
        // Arrange
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
            10,
            5,
            2);

        _flightRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Flight>
            {
            flight
            });

        var request = new FlightSearchRequest();

        // Act
        var result =
            await _flightService.SearchAvailableFlightsAsync(request);

        // Assert
        Assert.Equal(3, result.Count);

        Assert.Contains(
            result,
            item => item.TravelClass == TravelClass.Economy);

        Assert.Contains(
            result,
            item => item.TravelClass == TravelClass.Business);

        Assert.Contains(
            result,
            item => item.TravelClass == TravelClass.FirstClass);
    }

    [Theory]
    [InlineData(TravelClass.Economy, 0, 10, 10)]
    [InlineData(TravelClass.Business, 10, 0, 10)]
    [InlineData(TravelClass.FirstClass, 10, 10, 0)]
    public async Task SearchAvailableFlightsAsync_WhenSelectedClassHasNoSeats_ReturnsEmptyList(
    TravelClass travelClass,
    int economySeats,
    int businessSeats,
    int firstClassSeats)
    {
        // Arrange
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
            economySeats,
            businessSeats,
            firstClassSeats);

        _flightRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Flight>
            {
            flight
            });

        var request = new FlightSearchRequest
        {
            TravelClass = travelClass
        };

        // Act
        var result =
            await _flightService.SearchAvailableFlightsAsync(request);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAvailableFlightsAsync_WhenFlightFiltersAreProvided_ReturnsMatchingFlight()
    {
        // Arrange
        var matchingDepartureDate =
            DateTime.Today.AddDays(3).AddHours(10);

        var otherDepartureDate =
            DateTime.Today.AddDays(5).AddHours(12);

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

        _flightRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Flight>
            {
            matchingFlight,
            otherFlight
            });

        var request = new FlightSearchRequest
        {
            DepartureCountry = "  jordan  ",
            DestinationCountry = "uae",
            DepartureDate = matchingDepartureDate,
            DepartureAirport = "queen alia",
            ArrivalAirport = "DUBAI",
            TravelClass = TravelClass.Economy
        };

        // Act
        var result =
            await _flightService.SearchAvailableFlightsAsync(request);

        // Assert
        var returnedFlight = Assert.Single(result);

        Assert.Equal(matchingFlight.Id, returnedFlight.FlightId);
        Assert.Equal("RJ101", returnedFlight.FlightNumber);
    }

    [Fact]
    public async Task SearchAvailableFlightsAsync_WhenPriceExceedsMaximum_DoesNotReturnFlight()
    {
        // Arrange
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
            200,
            300,
            400,
            10,
            10,
            10);

        _flightRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Flight>
            {
            flight
            });

        var request = new FlightSearchRequest
        {
            TravelClass = TravelClass.Economy,
            MaxPrice = 150
        };

        // Act
        var result =
            await _flightService.SearchAvailableFlightsAsync(request);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAvailableFlightsAsync_WhenClassAndPriceMatch_ReturnsFlight()
    {
        // Arrange
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
            10,
            5,
            2);

        _flightRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Flight>
            {
            flight
            });

        var request = new FlightSearchRequest
        {
            TravelClass = TravelClass.Business,
            MaxPrice = 250
        };

        // Act
        var result =
            await _flightService.SearchAvailableFlightsAsync(request);

        // Assert
        var returnedFlight = Assert.Single(result);

        Assert.Equal(TravelClass.Business, returnedFlight.TravelClass);
        Assert.Equal(200, returnedFlight.Price);
        Assert.Equal(5, returnedFlight.AvailableSeats);
    }

    [Fact]
    public async Task SearchAvailableFlightsAsync_ReturnsResultsOrderedByDateThenPrice()
    {
        // Arrange
        var firstDepartureDate =
            DateTime.Today.AddDays(2).AddHours(10);

        var laterDepartureDate =
            DateTime.Today.AddDays(3).AddHours(10);

        var expensiveFirstFlight = new Flight(
            "RJ200",
            "Jordan",
            "UAE",
            "Queen Alia",
            "Dubai",
            firstDepartureDate,
            firstDepartureDate.AddHours(3),
            200,
            300,
            400,
            10,
            10,
            10);

        var cheaperFirstFlight = new Flight(
            "RJ100",
            "Jordan",
            "Egypt",
            "Queen Alia",
            "Cairo",
            firstDepartureDate,
            firstDepartureDate.AddHours(2),
            100,
            200,
            300,
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
            50,
            150,
            250,
            10,
            10,
            10);

        _flightRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Flight>
            {
            laterFlight,
            expensiveFirstFlight,
            cheaperFirstFlight
            });

        var request = new FlightSearchRequest
        {
            TravelClass = TravelClass.Economy
        };

        // Act
        var result =
            await _flightService.SearchAvailableFlightsAsync(request);

        // Assert
        Assert.Equal("RJ100", result[0].FlightNumber);
        Assert.Equal("RJ200", result[1].FlightNumber);
        Assert.Equal("RJ050", result[2].FlightNumber);
    }
}

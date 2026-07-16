using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Tests.Entities;

public class FlightTests
{
    [Fact]
    public void Constructor_WhenDataIsValid_CreatesFlight()
    {
        // Arrange
        var departureDate = DateTime.Now.AddDays(2);
        var arrivalDate = departureDate.AddHours(3);

        // Act
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

        // Assert
        Assert.Equal("RJ101", flight.FlightNumber);
        Assert.Equal(departureDate, flight.DepartureDate);
        Assert.Equal(arrivalDate, flight.ArrivalDate);
        Assert.Equal(100, flight.EconomyPrice);
        Assert.Equal(10, flight.EconomySeats);
    }

    [Fact]
    public void Constructor_WhenArrivalDateIsNotAfterDepartureDate_ThrowsArgumentException()
    {
        // Arrange
        var departureDate = DateTime.Now.AddDays(2);
        var arrivalDate = departureDate;

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            new Flight(
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
                10));

        // Assert
        Assert.Equal(
            "Arrival date must be after departure date.",
            exception.Message);
    }

    [Theory]
    [InlineData(-1, 200, 300)]
    [InlineData(100, -1, 300)]
    [InlineData(100, 200, -1)]
    public void Constructor_WhenAnyPriceIsNegative_ThrowsArgumentException(
        decimal economyPrice,
        decimal businessPrice,
        decimal firstClassPrice)
    {
        // Arrange
        var departureDate = DateTime.Now.AddDays(2);
        var arrivalDate = departureDate.AddHours(3);

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            new Flight(
                "RJ101",
                "Jordan",
                "UAE",
                "Queen Alia",
                "Dubai",
                departureDate,
                arrivalDate,
                economyPrice,
                businessPrice,
                firstClassPrice,
                10,
                10,
                10));

        // Assert
        Assert.Equal("Prices cannot be negative.", exception.Message);
    }
    [Theory]
    [InlineData(-1, 10, 10)]
    [InlineData(10, -1, 10)]
    [InlineData(10, 10, -1)]
    public void Constructor_WhenAnySeatCountIsNegative_ThrowsArgumentException(
        int economySeats,
        int businessSeats,
        int firstClassSeats)
    {
        // Arrange
        var departureDate = DateTime.Now.AddDays(2);
        var arrivalDate = departureDate.AddHours(3);

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            new Flight(
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
                firstClassSeats));

        // Assert
        Assert.Equal("Seats cannot be negative.", exception.Message);
    }
    [Theory]
    [InlineData(TravelClass.Economy, 100)]
    [InlineData(TravelClass.Business, 200)]
    [InlineData(TravelClass.FirstClass, 300)]
    public void GetPrice_WhenTravelClassIsValid_ReturnsCorrectPrice(
        TravelClass travelClass,
        decimal expectedPrice)
    {

        var departureDate = DateTime.Now.AddDays(2);
        var arrivalDate = departureDate.AddHours(3);

        // Arrange
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

        // Act
        var result = flight.GetPrice(travelClass);

        // Assert
        Assert.Equal(expectedPrice, result);
    }
    [Theory]
    [InlineData(TravelClass.Economy, 10)]
    [InlineData(TravelClass.Business, 5)]
    [InlineData(TravelClass.FirstClass, 2)]
    public void GetAvailableSeats_WhenTravelClassIsValid_ReturnsCorrectSeatCount(
        TravelClass travelClass,
        int expectedSeats)
    {
        var departureDate = DateTime.Now.AddDays(2);
        var arrivalDate = departureDate.AddHours(3);

        // Arrange
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

        // Act
        var result = flight.GetAvailableSeats(travelClass);

        // Assert
        Assert.Equal(expectedSeats, result);
    }
    [Theory]
    [InlineData(TravelClass.Economy)]
    [InlineData(TravelClass.Business)]
    [InlineData(TravelClass.FirstClass)]
    public void HasAvailableSeat_WhenSeatsExist_ReturnsTrue(
       TravelClass travelClass)
    {
        var departureDate = DateTime.Now.AddDays(2);
        var arrivalDate = departureDate.AddHours(3);

        // Arrange
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

        // Act
        var result = flight.HasAvailableSeat(travelClass);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(TravelClass.Economy, 9)]
    [InlineData(TravelClass.Business, 4)]
    [InlineData(TravelClass.FirstClass, 1)]
    public void ReserveSeat_WhenSeatIsAvailable_DecreasesSeatCount(
        TravelClass travelClass,
        int expectedSeats)
    {
        var departureDate = DateTime.Now.AddDays(2);
        var arrivalDate = departureDate.AddHours(3);

        // Arrange
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

        // Act
        flight.ReserveSeat(travelClass);

        // Assert
        Assert.Equal(
            expectedSeats,
            flight.GetAvailableSeats(travelClass));
    }

    [Fact]
    public void ReserveSeat_WhenNoSeatIsAvailable_ThrowsInvalidOperationException()
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
            0,
            10,
            10);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
            flight.ReserveSeat(TravelClass.Economy));

        // Assert
        Assert.Equal(
            "No available seats for the selected class.",
            exception.Message);
    }

    [Theory]
    [InlineData(TravelClass.Economy, 11)]
    [InlineData(TravelClass.Business, 6)]
    [InlineData(TravelClass.FirstClass, 3)]
    public void ReleaseSeat_IncreasesSeatCount(
        TravelClass travelClass,
        int expectedSeats)
    {
        var departureDate = DateTime.Now.AddDays(2);
        var arrivalDate = departureDate.AddHours(3);

        // Arrange
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

        // Act
        flight.ReleaseSeat(travelClass);

        // Assert
        Assert.Equal(
            expectedSeats,
            flight.GetAvailableSeats(travelClass));
    }

}

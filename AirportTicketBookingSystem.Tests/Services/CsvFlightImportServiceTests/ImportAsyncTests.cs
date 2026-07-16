using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.Domain.Entities;
using Moq;

namespace AirportTicketBookingSystem.Tests.Services.CsvFlightImportServiceTests;

public class ImportAsyncTests
{
    private readonly Mock<IFlightRepository> _flightRepositoryMock;
    private readonly CsvFlightImportService _service;

    public ImportAsyncTests()
    {
        _flightRepositoryMock = new Mock<IFlightRepository>();

        _service = new CsvFlightImportService(
            _flightRepositoryMock.Object);
    }

    [Fact]
    public async Task ImportAsync_WhenFilePathIsEmpty_ReturnsFileNotFoundError()
    {
        // Arrange
        var filePath = string.Empty;

        // Act
        var result = await _service.ImportAsync(filePath);

        // Assert
        Assert.Equal(
            "CSV file does not exist.",
            result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ImportAsync_WhenFileDoesNotExist_ReturnsError()
    {
        // Arrange
        var filePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.csv");

        // Act
        var result = await _service.ImportAsync(filePath);

        // Assert
        Assert.Equal(
            "CSV file does not exist.",
            result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ImportAsync_WhenFileContainsOnlyHeader_ReturnsEmptyFileError()
    {
        // Arrange
        var filePath = Path.GetTempFileName();

        const string header =
            "FlightNumber,DepartureCountry,DestinationCountry,DepartureDate," +
            "DepartureAirport,ArrivalAirport,ArrivalDate,EconomyPrice," +
            "BusinessPrice,FirstClassPrice,EconomySeats,BusinessSeats,FirstClassSeats";

        await File.WriteAllTextAsync(filePath, header);


        // Act
        var result = await _service.ImportAsync(filePath);

        // Assert
        Assert.Equal(
            "CSV file is empty.",
             result.Errors[0].ErrorMessage);

    }

    [Fact]
    public async Task ImportAsync_WhenRowHasInvalidColumnCount_ReturnsError()
    {
        // Arrange
        var filePath = Path.GetTempFileName();

        const string header =
            "FlightNumber,DepartureCountry,DestinationCountry,DepartureDate," +
            "DepartureAirport,ArrivalAirport,ArrivalDate,EconomyPrice," +
            "BusinessPrice,FirstClassPrice,EconomySeats,BusinessSeats,FirstClassSeats";

        const string invalidRow =
            "RJ101,Jordan,UAE";

        await File.WriteAllLinesAsync(
            filePath,
            new[]
            {
            header,
            invalidRow
            });

         // Act
         var result = await _service.ImportAsync(filePath);

         // Assert
         Assert.Equal(
             "Invalid column count. Expected 13 columns.",
              result.Errors[0].ErrorMessage);
      
    }

    [Fact]
    public async Task ImportAsync_WhenFlightNumberIsEmpty_ReturnsRequiredError()
    {
        // Arrange
        var filePath = Path.GetTempFileName();

        var departureDate = DateTime.Today.AddDays(2);
        var arrivalDate = departureDate.AddHours(3);

        const string header =
            "FlightNumber,DepartureCountry,DestinationCountry,DepartureDate," +
            "DepartureAirport,ArrivalAirport,ArrivalDate,EconomyPrice," +
            "BusinessPrice,FirstClassPrice,EconomySeats,BusinessSeats,FirstClassSeats";

        var row =
            $",Jordan,UAE,{departureDate:yyyy-MM-dd HH:mm}," +
            $"Queen Alia,Dubai,{arrivalDate:yyyy-MM-dd HH:mm}," +
            "100,200,300,10,10,10";

        await File.WriteAllLinesAsync(
            filePath,
            new[]
            {
            header,
            row
            });

        // Act
        var result = await _service.ImportAsync(filePath);

        // Assert
         Assert.Contains(
             result.Errors,
              error =>
               error.FieldName == "FlightNumber" &&
                 error.ErrorMessage == "FlightNumber is required.");
    }

    [Fact]
    public async Task ImportAsync_WhenDepartureDateIsInvalid_ReturnsError()
    {
        // Arrange
        var filePath = Path.GetTempFileName();

        var arrivalDate = DateTime.Today.AddDays(3);

        const string header =
            "FlightNumber,DepartureCountry,DestinationCountry,DepartureDate," +
            "DepartureAirport,ArrivalAirport,ArrivalDate,EconomyPrice," +
            "BusinessPrice,FirstClassPrice,EconomySeats,BusinessSeats,FirstClassSeats";

        var row =
            $"RJ101,Jordan,UAE,invalid-date," +
            $"Queen Alia,Dubai,{arrivalDate:yyyy-MM-dd HH:mm}," +
            "100,200,300,10,10,10";

        await File.WriteAllLinesAsync(
            filePath,
            new[]
            {
            header,
            row
            });

       // Act
       var result = await _service.ImportAsync(filePath);

      // Assert
       Assert.Contains(
          result.Errors,
           error =>
             error.FieldName == "DepartureDate" &&
               error.ErrorMessage == "Invalid departure date.");
    }

    [Fact]
    public async Task ImportAsync_WhenEconomyPriceIsNegative_ReturnsError()
    {
        // Arrange
        var filePath = Path.GetTempFileName();

        var departureDate = DateTime.Today.AddDays(2);
        var arrivalDate = departureDate.AddHours(3);

        const string header =
            "FlightNumber,DepartureCountry,DestinationCountry,DepartureDate," +
            "DepartureAirport,ArrivalAirport,ArrivalDate,EconomyPrice," +
            "BusinessPrice,FirstClassPrice,EconomySeats,BusinessSeats,FirstClassSeats";

        var row =
            $"RJ101,Jordan,UAE,{departureDate:yyyy-MM-dd HH:mm}," +
            $"Queen Alia,Dubai,{arrivalDate:yyyy-MM-dd HH:mm}," +
            "-100,200,300,10,10,10";

        await File.WriteAllLinesAsync(
            filePath,
            new[]
            {
            header,
            row
            });

        // Act
        var result = await _service.ImportAsync(filePath);

        // Assert
        Assert.Contains(
           result.Errors,
            error =>
              error.FieldName == "EconomyPrice" &&
               error.ErrorMessage == "Invalid economy price.");
    }

    [Fact]
    public async Task ImportAsync_WhenDepartureDateIsInPast_ReturnsError()
    {
        // Arrange
        var filePath = Path.GetTempFileName();

        var departureDate = DateTime.Today.AddDays(-2);
        var arrivalDate = departureDate.AddHours(3);

        const string header =
            "FlightNumber,DepartureCountry,DestinationCountry,DepartureDate," +
            "DepartureAirport,ArrivalAirport,ArrivalDate,EconomyPrice," +
            "BusinessPrice,FirstClassPrice,EconomySeats,BusinessSeats,FirstClassSeats";

        var row =
            $"RJ101,Jordan,UAE,{departureDate:yyyy-MM-dd HH:mm}," +
            $"Queen Alia,Dubai,{arrivalDate:yyyy-MM-dd HH:mm}," +
            "100,200,300,10,10,10";

        await File.WriteAllLinesAsync(
            filePath,
            new[]
            {
            header,
            row
            });

      // Act
      var result = await _service.ImportAsync(filePath);

      // Assert
       Assert.Contains(
          result.Errors,
           error =>
              error.FieldName == "DepartureDate" &&
                error.ErrorMessage ==
                 "Departure date must be today or in the future.");
    }

    [Fact]
    public async Task ImportAsync_WhenArrivalDateIsBeforeDepartureDate_ReturnsError()
    {
        // Arrange
        var filePath = Path.GetTempFileName();

        var departureDate = DateTime.Today.AddDays(3);
        var arrivalDate = departureDate.AddHours(-2);

        const string header =
            "FlightNumber,DepartureCountry,DestinationCountry,DepartureDate," +
            "DepartureAirport,ArrivalAirport,ArrivalDate,EconomyPrice," +
            "BusinessPrice,FirstClassPrice,EconomySeats,BusinessSeats,FirstClassSeats";

        var row =
            $"RJ101,Jordan,UAE,{departureDate:yyyy-MM-dd HH:mm}," +
            $"Queen Alia,Dubai,{arrivalDate:yyyy-MM-dd HH:mm}," +
            "100,200,300,10,10,10";

        await File.WriteAllLinesAsync(
            filePath,
            new[]
            {
            header,
            row
            });
     // Act
     var result = await _service.ImportAsync(filePath);

     // Assert
     Assert.Contains(
        result.Errors,
          error =>
            error.FieldName == "ArrivalDate" &&
              error.ErrorMessage ==
               "Arrival date must be after departure date.");
    }

    [Fact]
    public async Task ImportAsync_WhenFlightAlreadyExistsInStorage_ReturnsDuplicateError()
    {
        // Arrange
        var filePath = Path.GetTempFileName();

        var departureDate = DateTime.Today.AddDays(2);
        var arrivalDate = departureDate.AddHours(3);

        const string header =
            "FlightNumber,DepartureCountry,DestinationCountry,DepartureDate," +
            "DepartureAirport,ArrivalAirport,ArrivalDate,EconomyPrice," +
            "BusinessPrice,FirstClassPrice,EconomySeats,BusinessSeats,FirstClassSeats";

        var row =
            $"RJ101,Jordan,UAE,{departureDate:yyyy-MM-dd HH:mm}," +
            $"Queen Alia,Dubai,{arrivalDate:yyyy-MM-dd HH:mm}," +
            "100,200,300,10,10,10";

        var existingFlight = new Flight(
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
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(new List<Flight>
            {
            existingFlight
            });

        await File.WriteAllLinesAsync(
            filePath,
            new[]
            {
            header,
            row
            });
        // Act
        var result = await _service.ImportAsync(filePath);

        // Assert
        Assert.Contains(
           result.Errors,
             error =>
              error.FieldName == "FlightNumber" &&
               error.ErrorMessage == "Duplicate flight detected.");
    }

    [Fact]
    public async Task ImportAsync_WhenFlightIsDuplicatedInSameFile_ReturnsDuplicateError()
    {
        // Arrange
        var filePath = Path.GetTempFileName();

        var departureDate = DateTime.Today.AddDays(2);
        var arrivalDate = departureDate.AddHours(3);

        const string header =
            "FlightNumber,DepartureCountry,DestinationCountry,DepartureDate," +
            "DepartureAirport,ArrivalAirport,ArrivalDate,EconomyPrice," +
            "BusinessPrice,FirstClassPrice,EconomySeats,BusinessSeats,FirstClassSeats";

        var row =
            $"RJ101,Jordan,UAE,{departureDate:yyyy-MM-dd HH:mm}," +
            $"Queen Alia,Dubai,{arrivalDate:yyyy-MM-dd HH:mm}," +
            "100,200,300,10,10,10";

        _flightRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(new List<Flight>());

        await File.WriteAllLinesAsync(
            filePath,
            new[]
            {
            header,
            row,
            row
            });
        // Act
         var result = await _service.ImportAsync(filePath);
 
       // Assert
       Assert.Contains(
          result.Errors,
            error =>
              error.RowNumber == 3 &&
              error.ErrorMessage == "Duplicate flight detected.");
      
    }

    [Fact]
    public async Task ImportAsync_WhenCsvIsValid_ImportsFlightsSuccessfully()
    {
        // Arrange
        var filePath = Path.GetTempFileName();

        var departureDate = DateTime.Today.AddDays(2);
        var arrivalDate = departureDate.AddHours(3);

        const string header =
            "FlightNumber,DepartureCountry,DestinationCountry,DepartureDate," +
            "DepartureAirport,ArrivalAirport,ArrivalDate,EconomyPrice," +
            "BusinessPrice,FirstClassPrice,EconomySeats,BusinessSeats,FirstClassSeats";

        var row =
            $"RJ101,Jordan,UAE,{departureDate:yyyy-MM-dd HH:mm}," +
            $"Queen Alia,Dubai,{arrivalDate:yyyy-MM-dd HH:mm}," +
            "100,200,300,10,10,10";

        _flightRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(new List<Flight>());

        await File.WriteAllLinesAsync(
            filePath,
            new[]
            {
            header,
            row
            });

        
        // Act
        var result = await _service.ImportAsync(filePath);

       // Assert
       Assert.Equal(1, result.ImportedCount);
       Assert.Empty(result.Errors);

        
    }
}

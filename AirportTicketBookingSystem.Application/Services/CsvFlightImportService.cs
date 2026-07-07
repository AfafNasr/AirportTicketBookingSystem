using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Application.DTOs.Flights;
using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Application.Helpers;

namespace AirportTicketBookingSystem.Application.Services;

public sealed class CsvFlightImportService
{
    private readonly IFlightRepository _flightRepository;

    public CsvFlightImportService(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<FlightImportResult> ImportAsync(string filePath)
    {
        var result = new FlightImportResult();

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            AddError(result, 0, "File", "CSV file does not exist.");
            return result;
        }

        var lines = await File.ReadAllLinesAsync(filePath);

        if (lines.Length <= 1)
        {
            AddError(result, 0, "File", "CSV file is empty.");
            return result;
        }

        var existingFlights = await _flightRepository.GetAllAsync();
        var validFlightsToImport = new List<Flight>();

        for (var i = 1; i < lines.Length; i++)
        {
            var rowNumber = i + 1;
            var columns = CsvRowParser.ParseLine(lines[i]);

            if (columns.Count != 13)
            {
                AddError(result, rowNumber, "Row", "Invalid column count. Expected 13 columns.");
                continue;
            }

            var errorsBeforeCurrentRow = result.Errors.Count;

            ValidateRequired(result, rowNumber, "FlightNumber", columns[0]);
            ValidateRequired(result, rowNumber, "DepartureCountry", columns[1]);
            ValidateRequired(result, rowNumber, "DestinationCountry", columns[2]);
            ValidateRequired(result, rowNumber, "DepartureDate", columns[3]);
            ValidateRequired(result, rowNumber, "DepartureAirport", columns[4]);
            ValidateRequired(result, rowNumber, "ArrivalAirport", columns[5]);
            ValidateRequired(result, rowNumber, "ArrivalDate", columns[6]);

            var isDepartureDateValid = DateTime.TryParse(columns[3], out var departureDate);
            if (!isDepartureDateValid)
                AddError(result, rowNumber, "DepartureDate", "Invalid departure date.");

            var isArrivalDateValid = DateTime.TryParse(columns[6], out var arrivalDate);
            if (!isArrivalDateValid)
                AddError(result, rowNumber, "ArrivalDate", "Invalid arrival date.");

            var isEconomyPriceValid = decimal.TryParse(columns[7], out var economyPrice);
            if (!isEconomyPriceValid || economyPrice < 0)
                AddError(result, rowNumber, "EconomyPrice", "Invalid economy price.");

            var isBusinessPriceValid = decimal.TryParse(columns[8], out var businessPrice);
            if (!isBusinessPriceValid || businessPrice < 0)
                AddError(result, rowNumber, "BusinessPrice", "Invalid business price.");

            var isFirstClassPriceValid = decimal.TryParse(columns[9], out var firstClassPrice);
            if (!isFirstClassPriceValid || firstClassPrice < 0)
                AddError(result, rowNumber, "FirstClassPrice", "Invalid first class price.");

            var isEconomySeatsValid = int.TryParse(columns[10], out var economySeats);
            if (!isEconomySeatsValid || economySeats < 0)
                AddError(result, rowNumber, "EconomySeats", "Invalid economy seats.");

            var isBusinessSeatsValid = int.TryParse(columns[11], out var businessSeats);
            if (!isBusinessSeatsValid || businessSeats < 0)
                AddError(result, rowNumber, "BusinessSeats", "Invalid business seats.");

            var isFirstClassSeatsValid = int.TryParse(columns[12], out var firstClassSeats);
            if (!isFirstClassSeatsValid || firstClassSeats < 0)
                AddError(result, rowNumber, "FirstClassSeats", "Invalid first class seats.");

            if (isDepartureDateValid && departureDate.Date < DateTime.Today)
                AddError(result, rowNumber, "DepartureDate", "Departure date must be today or in the future.");

            if (isDepartureDateValid && isArrivalDateValid && arrivalDate <= departureDate)
                AddError(result, rowNumber, "ArrivalDate", "Arrival date must be after departure date.");

            if (result.Errors.Count > errorsBeforeCurrentRow)
                continue;

            var flightNumber = columns[0].Trim();

            var isDuplicateInStorage = existingFlights.Any(flight =>
                string.Equals(flight.FlightNumber, flightNumber, StringComparison.OrdinalIgnoreCase)
                && flight.DepartureDate == departureDate);

            var isDuplicateInCurrentFile = validFlightsToImport.Any(flight =>
                string.Equals(flight.FlightNumber, flightNumber, StringComparison.OrdinalIgnoreCase)
                && flight.DepartureDate == departureDate);

            if (isDuplicateInStorage || isDuplicateInCurrentFile)
            {
                AddError(result, rowNumber, "FlightNumber", "Duplicate flight detected.");
                continue;
            }

            var flight = new Flight(
                flightNumber,
                columns[1].Trim(),
                columns[2].Trim(),
                columns[4].Trim(),
                columns[5].Trim(),
                departureDate,
                arrivalDate,
                economyPrice,
                businessPrice,
                firstClassPrice,
                economySeats,
                businessSeats,
                firstClassSeats);

            validFlightsToImport.Add(flight);
        }

        if (validFlightsToImport.Count > 0)
        {
            await _flightRepository.AddRangeAsync(validFlightsToImport);
            result.ImportedCount = validFlightsToImport.Count;
        }

        return result;
    }

    private static void ValidateRequired(
        FlightImportResult result,
        int rowNumber,
        string fieldName,
        string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            return;

        AddError(result, rowNumber, fieldName, $"{fieldName} is required.");
    }

    private static void AddError(
        FlightImportResult result,
        int rowNumber,
        string fieldName,
        string errorMessage)
    {
        result.Errors.Add(new FlightImportError
        {
            RowNumber = rowNumber,
            FieldName = fieldName,
            ErrorMessage = errorMessage
        });
    }
}
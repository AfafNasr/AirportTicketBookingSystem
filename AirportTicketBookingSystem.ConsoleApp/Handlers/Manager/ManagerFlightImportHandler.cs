using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Input;

namespace AirportTicketBookingSystem.ConsoleApp.Handlers.Manager;

public sealed class ManagerFlightImportHandler
{
    private readonly CsvFlightImportService _csvFlightImportService;

    public ManagerFlightImportHandler(CsvFlightImportService csvFlightImportService)
    {
        _csvFlightImportService = csvFlightImportService;
    }

    public async Task ImportFlightsAsync()
    {
        ConsoleUi.Header("IMPORT FLIGHTS FROM CSV");

        var filePath = ConsoleUi.Prompt("CSV File Path");

        var result = await _csvFlightImportService.ImportAsync(filePath);

        Console.WriteLine();
        ConsoleUi.Success($"Imported Flights: {result.ImportedCount}");

        if (result.HasErrors)
        {
            ConsoleUi.Section("Validation Errors");

            var errorsByRow = result.Errors
                .GroupBy(error => error.RowNumber)
                .OrderBy(group => group.Key);

            foreach (var rowGroup in errorsByRow)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Row {rowGroup.Key}");
                Console.ResetColor();

                Console.WriteLine(new string('-', 50));

                foreach (var error in rowGroup)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"{error.FieldName}: ");
                    Console.ResetColor();

                    Console.WriteLine(error.ErrorMessage);
                }

                Console.WriteLine();
            }
        }

        ConsoleUi.Pause();
    }
}

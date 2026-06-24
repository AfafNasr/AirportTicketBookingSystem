using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.Infrastructure.Persistence;
using AirportTicketBookingSystem.Infrastructure.Security;
using AirportTicketBookingSystem.Infrastructure.Seed;

var userRepository = new UserRepository();
var flightRepository = new FlightRepository();
var passwordHasher = new Sha256PasswordHasher();

var managerSeeder = new ManagerSeeder(userRepository, passwordHasher);
await managerSeeder.SeedAsync();

Console.WriteLine("Manager seeded successfully.");
Console.WriteLine("Email: admin@airport.com");
Console.WriteLine("Password: Admin@123");
Console.WriteLine();

var importService = new CsvFlightImportService(flightRepository);

Console.Write("Enter CSV file path: ");
var filePath = Console.ReadLine();

var importResult = await importService.ImportAsync(filePath ?? string.Empty);

if (importResult.HasErrors)
{
    Console.WriteLine("Import failed:");

    foreach (var error in importResult.Errors)
    {
        Console.WriteLine($"Row {error.RowNumber} | {error.FieldName}: {error.ErrorMessage}");
    }
}
else
{
    Console.WriteLine($"Imported flights count: {importResult.ImportedCount}");
}

Console.ReadKey();
using AirportTicketBookingSystem.Application.DTOs.Auth;
using AirportTicketBookingSystem.Application.DTOs.Bookings;
using AirportTicketBookingSystem.Application.DTOs.Flights;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Session;
using AirportTicketBookingSystem.Domain.Enums;
using AirportTicketBookingSystem.Infrastructure.Persistence;
using AirportTicketBookingSystem.Infrastructure.Security;
using AirportTicketBookingSystem.Infrastructure.Seed;

var csvFilePath = @"C:\Users\AfafNasr\Desktop\flights.csv";

var userRepository = new UserRepository();
var passengerProfileRepository = new PassengerProfileRepository();
var flightRepository = new FlightRepository();
var bookingRepository = new BookingRepository();

var passwordHasher = new Sha256PasswordHasher();
var currentUserService = new CurrentUserService();

var managerSeeder = new ManagerSeeder(userRepository, passwordHasher);
await managerSeeder.SeedAsync();

Console.WriteLine("Manager seeded.");

var importService = new CsvFlightImportService(flightRepository);
var importResult = await importService.ImportAsync(csvFilePath);

if (importResult.HasErrors)
{
    Console.WriteLine("Flight import failed:");

    foreach (var error in importResult.Errors)
        Console.WriteLine($"Row {error.RowNumber} | {error.FieldName}: {error.ErrorMessage}");

    return;
}

Console.WriteLine($"Flights imported: {importResult.ImportedCount}");

var authService = new AuthService(
    userRepository,
    passengerProfileRepository,
    passwordHasher,
    currentUserService);

var passengerEmail = "passenger1@test.com";
var passengerPassword = "123456";

var registerResult = await authService.RegisterPassengerAsync(
    new RegisterPassengerRequest
    {
        FullName = "Passenger One",
        Email = passengerEmail,
        Password = passengerPassword,
        PassportNumber = "P111111"
    });

if (registerResult.IsFailure)
{
    var loginResult = await authService.LoginAsync(
        new LoginRequest
        {
            Email = passengerEmail,
            Password = passengerPassword
        });

    if (loginResult.IsFailure)
    {
        Console.WriteLine($"Login failed: {loginResult.Error}");
        return;
    }
}

Console.WriteLine($"Passenger logged in: {currentUserService.Email}");

var flightService = new FlightService(flightRepository);

var availableFlights = await flightService.SearchAvailableFlightsAsync(
    new FlightSearchRequest
    {
        DepartureCountry = "Jordan",
        TravelClass = TravelClass.Economy
    });

Console.WriteLine();
Console.WriteLine("Available Flights:");

for (var i = 0; i < availableFlights.Count; i++)
{
    var flight = availableFlights[i];

    Console.WriteLine(
        $"{i + 1}. {flight.FlightNumber} | {flight.DepartureCountry} -> {flight.DestinationCountry} | " +
        $"{flight.DepartureDate:g} | {flight.TravelClass} | {flight.Price} | Seats: {flight.AvailableSeats}");
}

if (availableFlights.Count == 0)
{
    Console.WriteLine("No available flights found.");
    return;
}

Console.Write("Choose flight number: ");
var input = Console.ReadLine();

if (!int.TryParse(input, out var selectedNumber) ||
    selectedNumber < 1 ||
    selectedNumber > availableFlights.Count)
{
    Console.WriteLine("Invalid selection.");
    return;
}

var selectedFlight = availableFlights[selectedNumber - 1];

var bookingService = new BookingService(
    bookingRepository,
    flightRepository,
    currentUserService);

var bookingResult = await bookingService.BookFlightAsync(
    new BookFlightRequest
    {
        FlightId = selectedFlight.FlightId,
        TravelClass = selectedFlight.TravelClass
    });

Console.WriteLine();

if (bookingResult.IsFailure)
{
    Console.WriteLine($"Booking failed: {bookingResult.Error}");
    return;
}

Console.WriteLine("Booking completed successfully.");
Console.WriteLine($"Booking Id: {bookingResult.Value!.BookingId}");
Console.WriteLine($"Flight: {bookingResult.Value.FlightNumber}");
Console.WriteLine($"Class: {bookingResult.Value.TravelClass}");
Console.WriteLine($"Price: {bookingResult.Value.Price}");

Console.WriteLine();
Console.WriteLine("My Bookings:");

var myBookings = await bookingService.GetMyBookingsAsync();

foreach (var booking in myBookings)
{
    Console.WriteLine(
        $"{booking.BookingId} | {booking.FlightNumber} | {booking.TravelClass} | {booking.Price} | {booking.Status}");
}

Console.ReadKey();
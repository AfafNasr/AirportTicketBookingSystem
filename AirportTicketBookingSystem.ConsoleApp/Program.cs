using System.Text;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Menus;
using AirportTicketBookingSystem.ConsoleApp.Session;
using AirportTicketBookingSystem.Infrastructure.Persistence;
using AirportTicketBookingSystem.Infrastructure.Security;
using AirportTicketBookingSystem.Infrastructure.Seed;


Console.OutputEncoding = Encoding.UTF8;
Console.Title = "Airport Ticket Booking System";

var userRepository = new UserRepository();
var passengerProfileRepository = new PassengerProfileRepository();
var flightRepository = new FlightRepository();
var bookingRepository = new BookingRepository();

var passwordHasher = new Sha256PasswordHasher();
var currentUserService = new CurrentUserService();

var managerSeeder = new ManagerSeeder(userRepository, passwordHasher);
await managerSeeder.SeedAsync();

var authService = new AuthService(
    userRepository,
    passengerProfileRepository,
    passwordHasher,
    currentUserService);

var flightService = new FlightService(flightRepository);

var bookingService = new BookingService(
    bookingRepository,
    flightRepository,
    userRepository,
    currentUserService);

var csvFlightImportService = new CsvFlightImportService(flightRepository);
var validationMetadataService = new FlightValidationMetadataService();

var passengerMenu = new PassengerMenu(
    flightService,
    bookingService,
    authService);

var managerMenu = new ManagerMenu(
    csvFlightImportService,
    validationMetadataService,
    bookingService,
    authService);

var mainMenu = new MainMenu(
    authService,
    passengerMenu,
    managerMenu);

await mainMenu.ShowAsync();
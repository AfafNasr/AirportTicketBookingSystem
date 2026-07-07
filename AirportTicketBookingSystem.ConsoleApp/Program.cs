using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Menus;
using AirportTicketBookingSystem.ConsoleApp.Session;
using AirportTicketBookingSystem.ConsoleApp.Workflows.Passenger;
using AirportTicketBookingSystem.ConsoleApp.Handlers.Manager;
using AirportTicketBookingSystem.Infrastructure.Persistence;
using AirportTicketBookingSystem.Infrastructure.Security;
using AirportTicketBookingSystem.Infrastructure.Seed;
using System.Text;


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
var managerFlightImportHandler =
    new ManagerFlightImportHandler(csvFlightImportService);

var passengerFlightSearchWorkflow = new PassengerFlightSearchHandler(flightService);
var passengerBookingWorkflow = new PassengerBookingHandler(bookingService);
var passengerBookingCancellationWorkflow = new PassengerBookingCancellationHandler(bookingService);
var passengerBookingViewerWorkflow =new PassengerBookingViewerHandler(bookingService);
var passengerAvailableFlightsWorkflow = new PassengerAvailableFlightsHandler(flightService, passengerBookingWorkflow);
var passengerSearchAndBookWorkflow =
    new PassengerSearchAndBookHandler(
        passengerFlightSearchWorkflow,
        passengerBookingWorkflow);
var passengerFlightGrouper = new PassengerFlightGrouper();
var passengerBookingModificationWorkflow =
    new PassengerBookingModificationHandler(
        bookingService,
        flightService,
        passengerFlightSearchWorkflow,
        passengerFlightGrouper);

var passengerMenu = new PassengerMenu(
    authService,
    passengerBookingCancellationWorkflow,
    passengerBookingViewerWorkflow,
    passengerAvailableFlightsWorkflow,
    passengerSearchAndBookWorkflow,
    passengerFlightGrouper,
    passengerBookingModificationWorkflow);

var managerMenu = new ManagerMenu(
    validationMetadataService,
    bookingService,
    authService,
    managerFlightImportHandler);

var mainMenu = new MainMenu(
    authService,
    passengerMenu,
    managerMenu);

await mainMenu.ShowAsync();
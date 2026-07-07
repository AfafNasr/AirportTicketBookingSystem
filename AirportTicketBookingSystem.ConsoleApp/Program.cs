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
var currentUserService = new CurrentUserSession();

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
var managerFlightImportHandler =new ManagerFlightImportHandler(csvFlightImportService);
var managerValidationRulesHandler =new ManagerValidationRulesHandler(validationMetadataService);
var managerBookingFilterHandler =new ManagerBookingFilterHandler(bookingService);

var passengerFlightSearchHandler = new PassengerFlightSearchHandler(flightService);
var passengerBookingHandler = new PassengerBookingHandler(bookingService);
var passengerBookingCancellationHandler = new PassengerBookingCancellationHandler(bookingService);
var passengerBookingViewerHandler = new PassengerBookingViewerHandler(bookingService);
var passengerAvailableFlightsHandler = new PassengerAvailableFlightsHandler(flightService, passengerBookingHandler);
var passengerSearchAndBookHandler = new PassengerSearchAndBookHandler(passengerFlightSearchHandler,passengerBookingHandler);
var passengerFlightGrouper = new PassengerFlightGrouper();
var passengerBookingModificationWorkflow = new PassengerBookingModificationHandler(
        bookingService,
        flightService,
        passengerFlightSearchHandler,
        passengerFlightGrouper);

var passengerMenu = new PassengerMenu(
    authService,
    passengerBookingCancellationHandler,
    passengerBookingViewerHandler,
    passengerAvailableFlightsHandler,
    passengerSearchAndBookHandler,
    passengerFlightGrouper,
    passengerBookingModificationWorkflow);

var managerMenu = new ManagerMenu(
    authService,
    managerFlightImportHandler,
    managerValidationRulesHandler,
     managerBookingFilterHandler);

var mainMenu = new MainMenu(
    authService,
    passengerMenu,
    managerMenu);

await mainMenu.ShowAsync();
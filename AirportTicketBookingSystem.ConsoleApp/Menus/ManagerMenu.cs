
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Input;
using AirportTicketBookingSystem.ConsoleApp.Handlers.Manager;

namespace AirportTicketBookingSystem.ConsoleApp.Menus
{
    public sealed class ManagerMenu
    {
        private readonly AuthService _authService;
        private readonly ManagerFlightImportHandler _flightImportHandler;
        private readonly ManagerValidationRulesHandler _validationRulesHandler;
        private readonly ManagerBookingFilterHandler _bookingFilterHandler;

        public ManagerMenu(
            AuthService authService,
            ManagerFlightImportHandler flightImportHandler,
            ManagerValidationRulesHandler validationRulesHandler,
            ManagerBookingFilterHandler bookingFilterHandler)
        {
            _authService = authService;
            _flightImportHandler = flightImportHandler;
            _validationRulesHandler = validationRulesHandler;
            _bookingFilterHandler = bookingFilterHandler;
        }

        public async Task ShowAsync()
        {
            while (true)
            {
                ConsoleUi.Header("MANAGER MENU");

                Console.WriteLine("1. Import Flights From CSV");
                Console.WriteLine("2. View Flight Validation Rules");
                Console.WriteLine("3. Filter Bookings");
                Console.WriteLine("4. Logout");
                Console.WriteLine();

                var choice = ConsoleUi.Prompt("Choose option");

                switch (choice)
                {
                    case "1":
                        await _flightImportHandler.ImportFlightsAsync();
                        break;

                    case "2":
                        _validationRulesHandler.ViewValidationRules();
                        break;

                    case "3":
                        await _bookingFilterHandler.FilterBookingsAsync();
                        break;

                    case "4":
                        _authService.Logout();
                        return;

                    default:
                        ConsoleUi.Error("Invalid option.");
                        ConsoleUi.Pause();
                        break;
                }
            }
        }

       
    }
}

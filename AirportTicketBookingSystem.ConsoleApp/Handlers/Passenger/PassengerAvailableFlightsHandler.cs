using AirportTicketBookingSystem.Application.DTOs.Flights;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Input;

namespace AirportTicketBookingSystem.ConsoleApp.Workflows.Passenger
{
    public sealed class PassengerAvailableFlightsHandler
    {
        private readonly FlightService _flightService;
        private readonly PassengerBookingHandler _bookingWorkflow;

        public PassengerAvailableFlightsHandler(
            FlightService flightService,
            PassengerBookingHandler bookingWorkflow)
        {
            _flightService = flightService;
            _bookingWorkflow = bookingWorkflow;
        }

        public async Task ViewAllAvailableFlightsAsync(Action<IReadOnlyList<FlightSearchResult>> printFlights)
        {
            ConsoleUi.Header("ALL AVAILABLE FLIGHTS");

            var flights = await _flightService.SearchAvailableFlightsAsync(
                new FlightSearchRequest());

            printFlights(flights);

            if (flights.Count == 0)
            {
                ConsoleUi.Pause();
                return;
            }

            var answer = ConsoleUi.Prompt("Do you want to book one of these flights? (Y/N)");

            if (!answer.Equals("Y", StringComparison.OrdinalIgnoreCase))
                return;

            await _bookingWorkflow.BookSelectedFlightAsync(flights);
        }
    }
}

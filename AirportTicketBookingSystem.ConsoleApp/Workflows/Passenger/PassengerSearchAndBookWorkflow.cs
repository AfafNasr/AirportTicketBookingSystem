using AirportTicketBookingSystem.Application.DTOs.Flights;
using AirportTicketBookingSystem.ConsoleApp.Input;

namespace AirportTicketBookingSystem.ConsoleApp.Workflows.Passenger
{
    public sealed class PassengerSearchAndBookWorkflow
    {
        private readonly PassengerFlightSearchWorkflow _flightSearchWorkflow;
        private readonly PassengerBookingWorkflow _bookingWorkflow;

        public PassengerSearchAndBookWorkflow(
            PassengerFlightSearchWorkflow flightSearchWorkflow,
            PassengerBookingWorkflow bookingWorkflow)
        {
            _flightSearchWorkflow = flightSearchWorkflow;
            _bookingWorkflow = bookingWorkflow;
        }

        public async Task SearchAndBookFlightAsync(Action<IReadOnlyList<FlightSearchResult>> printFlights)
        {
            ConsoleUi.Header("SEARCH & BOOK FLIGHT");

            var flights = await _flightSearchWorkflow.SearchFlightsWithoutPauseAsync();

            printFlights(flights);

            if (flights.Count == 0)
            {
                ConsoleUi.Error("No available flights found.");
                ConsoleUi.Pause();
                return;
            }

            await _bookingWorkflow.BookSelectedFlightAsync(flights);
        }
    }
}

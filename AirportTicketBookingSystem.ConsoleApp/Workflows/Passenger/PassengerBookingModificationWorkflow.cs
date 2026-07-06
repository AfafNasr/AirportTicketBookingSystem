using AirportTicketBookingSystem.Application.DTOs.Bookings;
using AirportTicketBookingSystem.Application.DTOs.Flights;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Input;
using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.ConsoleApp.Workflows.Passenger
{
    public sealed class PassengerBookingModificationWorkflow
    {
        private readonly BookingService _bookingService;
        private readonly FlightService _flightService;
        private readonly PassengerFlightSearchWorkflow _flightSearchWorkflow;
        private readonly PassengerFlightGrouper _flightGrouper;

        public PassengerBookingModificationWorkflow(
            BookingService bookingService,
            FlightService flightService,
            PassengerFlightSearchWorkflow flightSearchWorkflow,
             PassengerFlightGrouper flightGrouper)
        {
            _bookingService = bookingService;
            _flightService = flightService;
            _flightSearchWorkflow = flightSearchWorkflow;
            _flightGrouper = flightGrouper;
        }

        public async Task ModifyBookingAsync(
            Action<IReadOnlyList<BookingResponse>> printBookings,
            Action<IReadOnlyList<FlightSearchResult>> printFlights)
        {
            ConsoleUi.Header("MODIFY BOOKING");

            var bookings = (await _bookingService.GetMyBookingsAsync())
                .Where(booking => booking.Status == BookingStatus.Active)
                .ToList();

            printBookings(bookings);

            if (bookings.Count == 0)
            {
                ConsoleUi.Pause();
                return;
            }

            var bookingInput = ConsoleUi.Prompt("Select booking number to modify");

            if (!int.TryParse(bookingInput, out var selectedBookingIndex) ||
                selectedBookingIndex < 1 ||
                selectedBookingIndex > bookings.Count)
            {
                ConsoleUi.Error("Invalid booking selection.");
                ConsoleUi.Pause();
                return;
            }

            var selectedBooking = bookings[selectedBookingIndex - 1];

            ConsoleUi.Section("Modification Type");
            Console.WriteLine("1. Change Class only");
            Console.WriteLine("2. Change Flight and Class");
            Console.WriteLine("0. Back");

            var choice = ConsoleUi.Prompt("Choose option");

            if (choice == "0")
                return;

            if (choice == "1")
            {
                await ModifyBookingClassOnlyAsync(selectedBooking);
                return;
            }

            if (choice == "2")
            {
                await ModifyBookingFlightAndClassAsync(selectedBooking, printFlights);
                return;
            }

            ConsoleUi.Error("Invalid option.");
            ConsoleUi.Pause();
        }

        private async Task ModifyBookingClassOnlyAsync(BookingResponse selectedBooking)
        {
            ConsoleUi.Section("Choose New Class");

            var flights = await _flightService.SearchAvailableFlightsAsync(
                new FlightSearchRequest
                {
                    DepartureCountry = selectedBooking.DepartureCountry,
                    DestinationCountry = selectedBooking.DestinationCountry,
                    DepartureDate = selectedBooking.DepartureDate,
                    DepartureAirport = selectedBooking.DepartureAirport,
                    ArrivalAirport = selectedBooking.ArrivalAirport
                });

            var sameFlightOptions = flights
                .Where(flight => flight.FlightId == selectedBooking.FlightId)
                .OrderBy(flight => flight.TravelClass)
                .ToList();

            if (sameFlightOptions.Count == 0)
            {
                ConsoleUi.Error("No class options found for this flight.");
                ConsoleUi.Pause();
                return;
            }

            for (var i = 0; i < sameFlightOptions.Count; i++)
            {
                var option = sameFlightOptions[i];

                Console.WriteLine(
                    $"{i + 1}. {option.TravelClass,-10} | Price: {option.Price,-8} | Seats: {option.AvailableSeats}");
            }

            var input = ConsoleUi.Prompt("Select new class");

            if (!int.TryParse(input, out var selectedClassIndex) ||
                selectedClassIndex < 1 ||
                selectedClassIndex > sameFlightOptions.Count)
            {
                ConsoleUi.Error("Invalid class selection.");
                ConsoleUi.Pause();
                return;
            }

            var selectedOption = sameFlightOptions[selectedClassIndex - 1];

            if (selectedOption.TravelClass == selectedBooking.TravelClass)
            {
                ConsoleUi.Error("You selected the same class. No changes were made.");
                ConsoleUi.Pause();
                return;
            }

            ConsoleUi.Section("Modification Confirmation");
            Console.WriteLine($"Flight       : {selectedBooking.FlightNumber}");
            Console.WriteLine($"Route        : {selectedBooking.DepartureCountry} -> {selectedBooking.DestinationCountry}");
            Console.WriteLine($"Old Class    : {selectedBooking.TravelClass}");
            Console.WriteLine($"New Class    : {selectedOption.TravelClass}");
            Console.WriteLine($"New Price    : {selectedOption.Price}");

            var confirm = ConsoleUi.Prompt("Confirm modification? (Y/N)");

            if (!confirm.Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleUi.Info("Modification cancelled.");
                ConsoleUi.Pause();
                return;
            }

            var result = await _bookingService.ModifyBookingAsync(new ModifyBookingRequest
            {
                BookingId = selectedBooking.BookingId,
                NewFlightId = selectedBooking.FlightId,
                NewTravelClass = selectedOption.TravelClass
            });

            if (result.IsFailure)
                ConsoleUi.Error(result.Error);
            else
                ConsoleUi.Success("Booking class modified successfully.");

            ConsoleUi.Pause();
        }

        private async Task ModifyBookingFlightAndClassAsync(
    BookingResponse selectedBooking,
    Action<IReadOnlyList<FlightSearchResult>> printFlights)
        {
            ConsoleUi.Section("Choose New Flight");

            var flights = await _flightSearchWorkflow.SearchFlightsWithoutPauseAsync();

            printFlights(flights);

            if (flights.Count == 0)
            {
                ConsoleUi.Error("No flights found.");
                ConsoleUi.Pause();
                return;
            }

            var groupedFlights = _flightGrouper.GroupFlights(flights);

            var flightInput = ConsoleUi.Prompt("Select new flight option number");

            if (!int.TryParse(flightInput, out var selectedFlightIndex) ||
                selectedFlightIndex < 1 ||
                selectedFlightIndex > groupedFlights.Count)
            {
                ConsoleUi.Error("Invalid flight selection.");
                ConsoleUi.Pause();
                return;
            }

            var selectedFlight = groupedFlights[selectedFlightIndex - 1];

            ConsoleUi.Section("Choose New Class");

            for (var i = 0; i < selectedFlight.ClassOptions.Count; i++)
            {
                var option = selectedFlight.ClassOptions[i];

                Console.WriteLine(
                    $"{i + 1}. {option.TravelClass,-10} | Price: {option.Price,-8} | Seats: {option.AvailableSeats}");
            }

            var classInput = ConsoleUi.Prompt("Select new class number");

            if (!int.TryParse(classInput, out var selectedClassIndex) ||
                selectedClassIndex < 1 ||
                selectedClassIndex > selectedFlight.ClassOptions.Count)
            {
                ConsoleUi.Error("Invalid class selection.");
                ConsoleUi.Pause();
                return;
            }

            var selectedOption = selectedFlight.ClassOptions[selectedClassIndex - 1];

            ConsoleUi.Section("Modification Confirmation");
            Console.WriteLine($"Old Flight   : {selectedBooking.FlightNumber}");
            Console.WriteLine($"Old Route    : {selectedBooking.DepartureCountry} -> {selectedBooking.DestinationCountry}");
            Console.WriteLine($"Old Class    : {selectedBooking.TravelClass}");
            Console.WriteLine();
            Console.WriteLine($"New Flight   : {selectedOption.FlightNumber}");
            Console.WriteLine($"New Route    : {selectedOption.DepartureCountry} -> {selectedOption.DestinationCountry}");
            Console.WriteLine($"New Class    : {selectedOption.TravelClass}");
            Console.WriteLine($"New Price    : {selectedOption.Price}");

            var confirm = ConsoleUi.Prompt("Confirm modification? (Y/N)");

            if (!confirm.Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleUi.Info("Modification cancelled.");
                ConsoleUi.Pause();
                return;
            }

            var result = await _bookingService.ModifyBookingAsync(new ModifyBookingRequest
            {
                BookingId = selectedBooking.BookingId,
                NewFlightId = selectedOption.FlightId,
                NewTravelClass = selectedOption.TravelClass
            });

            if (result.IsFailure)
                ConsoleUi.Error(result.Error);
            else
                ConsoleUi.Success("Booking modified successfully.");

            ConsoleUi.Pause();
        }
    }
}

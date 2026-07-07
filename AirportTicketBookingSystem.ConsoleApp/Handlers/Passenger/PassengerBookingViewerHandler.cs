using AirportTicketBookingSystem.Application.DTOs.Bookings;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Input;

namespace AirportTicketBookingSystem.ConsoleApp.Workflows.Passenger
{
    public sealed class PassengerBookingViewerHandler
    {
        private readonly BookingService _bookingService;

        public PassengerBookingViewerHandler(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task ViewMyBookingsAsync(Action<IReadOnlyList<BookingResponse>> printBookings)
        {
            ConsoleUi.Header("MY BOOKINGS");

            var bookings = await _bookingService.GetMyBookingsAsync();

            printBookings(bookings);

            ConsoleUi.Pause();
        }

    }
}

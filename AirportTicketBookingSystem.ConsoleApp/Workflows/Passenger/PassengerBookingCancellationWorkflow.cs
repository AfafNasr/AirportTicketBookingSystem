using AirportTicketBookingSystem.Application.DTOs.Bookings;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Input;
using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.ConsoleApp.Workflows.Passenger
{
    public sealed class PassengerBookingCancellationWorkflow
    {
        private readonly BookingService _bookingService;

        public PassengerBookingCancellationWorkflow(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task CancelBookingAsync(Action<IReadOnlyList<BookingResponse>> printBookings)
        {
            ConsoleUi.Header("CANCEL BOOKING");

            var bookings = (await _bookingService.GetMyBookingsAsync())
                .Where(booking => booking.Status == BookingStatus.Active)
                .ToList();

            printBookings(bookings);

            if (bookings.Count == 0)
            {
                ConsoleUi.Pause();
                return;
            }

            var input = ConsoleUi.Prompt("Select booking number to cancel");

            if (!int.TryParse(input, out var selectedIndex) ||
                selectedIndex < 1 ||
                selectedIndex > bookings.Count)
            {
                ConsoleUi.Error("Invalid booking selection.");
                ConsoleUi.Pause();
                return;
            }

            var bookingId = bookings[selectedIndex - 1].BookingId;

            var result = await _bookingService.CancelBookingAsync(bookingId);

            if (result.IsFailure)
                ConsoleUi.Error(result.Error);
            else
                ConsoleUi.Success("Booking cancelled successfully.");

            ConsoleUi.Pause();
        }
    }
}

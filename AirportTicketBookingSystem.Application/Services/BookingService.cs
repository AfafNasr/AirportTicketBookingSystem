using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Application.Abstractions.Services;
using AirportTicketBookingSystem.Application.Common;
using AirportTicketBookingSystem.Application.DTOs.Bookings;
using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Application.Services
{
    public sealed class BookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly ICurrentUserService _currentUserService;

        public BookingService(
            IBookingRepository bookingRepository,
            IFlightRepository flightRepository,
            ICurrentUserService currentUserService)
        {
            _bookingRepository = bookingRepository;
            _flightRepository = flightRepository;
            _currentUserService = currentUserService;
        }

        public async Task<Result<BookingResponse>> BookFlightAsync(BookFlightRequest request)
        {
            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
                return Result<BookingResponse>.Failure("You must login before booking a flight.");

            if (_currentUserService.Role != UserRole.Passenger)
                return Result<BookingResponse>.Failure("Only passengers can book flights.");

            var flight = await _flightRepository.GetByIdAsync(request.FlightId);

            if (flight is null)
                return Result<BookingResponse>.Failure("Flight was not found.");

            if (flight.DepartureDate <= DateTime.Now)
                return Result<BookingResponse>.Failure("Cannot book a departed flight.");

            if (!flight.HasAvailableSeat(request.TravelClass))
                return Result<BookingResponse>.Failure("No available seats for the selected class.");

            var price = flight.GetPrice(request.TravelClass);
            flight.ReserveSeat(request.TravelClass);

            var booking = new Booking(
                _currentUserService.UserId.Value,
                flight.Id,
                request.TravelClass,
                price);

            await _bookingRepository.AddAsync(booking);

            await _flightRepository.UpdateAsync(flight);

            return Result<BookingResponse>.Success(new BookingResponse
            {
                BookingId = booking.Id,
                FlightId = flight.Id,
                FlightNumber = flight.FlightNumber,
                TravelClass = booking.TravelClass,
                Price = booking.Price,
                BookingDate = booking.BookingDate,
                Status = booking.Status
            });
        }

        public async Task<IReadOnlyList<BookingResponse>> GetMyBookingsAsync()
        {
            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
                return [];

            var bookings = await _bookingRepository.GetAllAsync();
            var flights = await _flightRepository.GetAllAsync();

            return bookings
                .Where(booking => booking.PassengerUserId == _currentUserService.UserId.Value)
                .Select(booking =>
                {
                    var flight = flights.FirstOrDefault(f => f.Id == booking.FlightId);

                    return new BookingResponse
                    {
                        BookingId = booking.Id,
                        FlightId = booking.FlightId,
                        FlightNumber = flight?.FlightNumber ?? "Unknown",
                        TravelClass = booking.TravelClass,
                        Price = booking.Price,
                        BookingDate = booking.BookingDate,
                        Status = booking.Status
                    };
                })
                .OrderByDescending(booking => booking.BookingDate)
                .ToList();
        }

        public async Task<Result> CancelBookingAsync(Guid bookingId)
        {
            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
                return Result.Failure("You must login before cancelling a booking.");

            if (_currentUserService.Role != UserRole.Passenger)
                return Result.Failure("Only passengers can cancel their bookings.");

            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking is null)
                return Result.Failure("Booking was not found.");

            if (booking.PassengerUserId != _currentUserService.UserId.Value)
                return Result.Failure("You are not allowed to cancel this booking.");

            if (booking.Status == BookingStatus.Cancelled)
                return Result.Failure("Booking is already cancelled.");

            booking.Cancel();

            await _bookingRepository.UpdateAsync(booking);

            return Result.Success();
        }
    }
}

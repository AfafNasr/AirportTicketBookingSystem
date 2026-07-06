

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
        private readonly IUserRepository _userRepository;

        public BookingService(
            IBookingRepository bookingRepository,
            IFlightRepository flightRepository,
             IUserRepository userRepository,
            ICurrentUserService currentUserService)
        {
            _bookingRepository = bookingRepository;
            _flightRepository = flightRepository;
            _userRepository = userRepository;
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
                DepartureCountry = flight.DepartureCountry,
                DestinationCountry = flight.DestinationCountry,
                DepartureAirport = flight.DepartureAirport,
                ArrivalAirport = flight.ArrivalAirport,
                DepartureDate = flight.DepartureDate,
                ArrivalDate = flight.ArrivalDate,
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
                        DepartureCountry = flight?.DepartureCountry ?? "Unknown",
                        DestinationCountry = flight?.DestinationCountry ?? "Unknown",
                        DepartureAirport = flight?.DepartureAirport ?? "Unknown",
                        ArrivalAirport = flight?.ArrivalAirport ?? "Unknown",
                        DepartureDate = flight?.DepartureDate ?? default,
                        ArrivalDate = flight?.ArrivalDate ?? default,
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

            var flight = await _flightRepository.GetByIdAsync(booking.FlightId);

            if (flight is null)
                return Result.Failure("Flight was not found.");

            flight.ReleaseSeat(booking.TravelClass);

            await _flightRepository.UpdateAsync(flight);

            booking.Cancel();

            await _bookingRepository.UpdateAsync(booking);

            return Result.Success();
        }

        public async Task<Result> ModifyBookingAsync(ModifyBookingRequest request)
        {
            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
                return Result.Failure("You must login before modifying a booking.");

            if (_currentUserService.Role != UserRole.Passenger)
                return Result.Failure("Only passengers can modify their bookings.");

            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);

            if (booking is null)
                return Result.Failure("Booking was not found.");

            if (booking.PassengerUserId != _currentUserService.UserId.Value)
                return Result.Failure("You are not allowed to modify this booking.");

            if (booking.Status == BookingStatus.Cancelled)
                return Result.Failure("Cancelled bookings cannot be modified.");

            var oldFlight = await _flightRepository.GetByIdAsync(booking.FlightId);

            if (oldFlight is null)
                return Result.Failure("Original flight was not found.");

            var newFlight = await _flightRepository.GetByIdAsync(request.NewFlightId);

            if (newFlight is null)
                return Result.Failure("New flight was not found.");

            if (newFlight.DepartureDate <= DateTime.Now)
                return Result.Failure("Cannot change booking to a departed flight.");

            var isSameFlight = oldFlight.Id == newFlight.Id;
            var isSameClass = booking.TravelClass == request.NewTravelClass;

            if (isSameFlight && isSameClass)
                return Result.Failure("No changes were made.");

            if (isSameFlight)
            {
                oldFlight.ReleaseSeat(booking.TravelClass);

                if (!oldFlight.HasAvailableSeat(request.NewTravelClass))
                {
                    oldFlight.ReserveSeat(booking.TravelClass);
                    return Result.Failure("No available seats for the selected class.");
                }

                oldFlight.ReserveSeat(request.NewTravelClass);

                await _flightRepository.UpdateAsync(oldFlight);
            }
            else
            {
                if (!newFlight.HasAvailableSeat(request.NewTravelClass))
                    return Result.Failure("No available seats for the selected class.");

                oldFlight.ReleaseSeat(booking.TravelClass);
                newFlight.ReserveSeat(request.NewTravelClass);

                await _flightRepository.UpdateAsync(oldFlight);
                await _flightRepository.UpdateAsync(newFlight);
            }

            var newPrice = newFlight.GetPrice(request.NewTravelClass);

            booking.Modify(
                newFlight.Id,
                request.NewTravelClass,
                newPrice);

            await _bookingRepository.UpdateAsync(booking);

            return Result.Success();
        }

        public async Task<IReadOnlyList<ManagerBookingResult>> FilterBookingsAsync(BookingFilterRequest request)
        {
            if (!_currentUserService.IsAuthenticated ||
                _currentUserService.Role != UserRole.Manager)
            {
                return [];
            }

            var bookings = await _bookingRepository.GetAllAsync();
            var flights = await _flightRepository.GetAllAsync();
            var users = await _userRepository.GetAllAsync();

            var query =
                from booking in bookings
                join flight in flights on booking.FlightId equals flight.Id
                join user in users on booking.PassengerUserId equals user.Id
                select new
                {
                    Booking = booking,
                    Flight = flight,
                    Passenger = user
                };

            if (!string.IsNullOrWhiteSpace(request.FlightNumber))
            {
                query = query.Where(item =>
                    item.Flight.FlightNumber.Contains(
                        request.FlightNumber.Trim(),
                        StringComparison.OrdinalIgnoreCase));
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(item =>
                    item.Booking.Price <= request.MaxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.DepartureCountry))
            {
                query = query.Where(item =>
                    item.Flight.DepartureCountry.Contains(
                        request.DepartureCountry.Trim(),
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(request.DestinationCountry))
            {
                query = query.Where(item =>
                    item.Flight.DestinationCountry.Contains(
                        request.DestinationCountry.Trim(),
                        StringComparison.OrdinalIgnoreCase));
            }

            if (request.DepartureDate.HasValue)
            {
                query = query.Where(item =>
                    item.Flight.DepartureDate.Date ==
                    request.DepartureDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(request.DepartureAirport))
            {
                query = query.Where(item =>
                    item.Flight.DepartureAirport.Contains(
                        request.DepartureAirport.Trim(),
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(request.ArrivalAirport))
            {
                query = query.Where(item =>
                    item.Flight.ArrivalAirport.Contains(
                        request.ArrivalAirport.Trim(),
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(request.PassengerEmail))
            {
                query = query.Where(item =>
                    item.Passenger.Email.Contains(
                        request.PassengerEmail.Trim(),
                        StringComparison.OrdinalIgnoreCase));
            }

            if (request.TravelClass.HasValue)
            {
                query = query.Where(item =>
                    item.Booking.TravelClass == request.TravelClass.Value);
            }

            return query
                .Select(item => new ManagerBookingResult
                {
                    BookingId = item.Booking.Id,
                    PassengerName = item.Passenger.FullName,
                    PassengerEmail = item.Passenger.Email,
                    FlightNumber = item.Flight.FlightNumber,
                    Price = item.Booking.Price,
                    DepartureCountry = item.Flight.DepartureCountry,
                    DestinationCountry = item.Flight.DestinationCountry,
                    DepartureDate = item.Flight.DepartureDate,
                    DepartureAirport = item.Flight.DepartureAirport,
                    ArrivalAirport = item.Flight.ArrivalAirport,
                    TravelClass = item.Booking.TravelClass,
                    Status = item.Booking.Status
                })
                .OrderBy(result => result.DepartureDate)
                .ThenBy(result => result.FlightNumber)
                .ToList();
        }
    }
}

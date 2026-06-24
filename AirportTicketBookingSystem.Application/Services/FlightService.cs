using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Application.DTOs.Flights;
using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Application.Services
{
    public sealed class FlightService
    {
        private readonly IFlightRepository _flightRepository;

        public FlightService(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public async Task<IReadOnlyList<FlightSearchResult>> SearchAvailableFlightsAsync(
            FlightSearchRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var flights = await _flightRepository.GetAllAsync();

            var query = flights
                .Where(flight => flight.DepartureDate >= DateTime.Now)
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(request.DepartureCountry))
            {
                query = query.Where(flight =>
                    ContainsIgnoreCase(flight.DepartureCountry, request.DepartureCountry));
            }

            if (!string.IsNullOrWhiteSpace(request.DestinationCountry))
            {
                query = query.Where(flight =>
                    ContainsIgnoreCase(flight.DestinationCountry, request.DestinationCountry));
            }

            if (request.DepartureDate.HasValue)
            {
                query = query.Where(flight =>
                    flight.DepartureDate.Date == request.DepartureDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(request.DepartureAirport))
            {
                query = query.Where(flight =>
                    ContainsIgnoreCase(flight.DepartureAirport, request.DepartureAirport));
            }

            if (!string.IsNullOrWhiteSpace(request.ArrivalAirport))
            {
                query = query.Where(flight =>
                    ContainsIgnoreCase(flight.ArrivalAirport, request.ArrivalAirport));
            }

            var results = new List<FlightSearchResult>();

            foreach (var flight in query)
            {
                var travelClasses = request.TravelClass.HasValue
                    ? [request.TravelClass.Value]
                    : new[] { TravelClass.Economy, TravelClass.Business, TravelClass.FirstClass };

                foreach (var travelClass in travelClasses)
                {
                    var availableSeats = GetAvailableSeats(flight, travelClass);

                    if (availableSeats <= 0)
                        continue;

                    var price = flight.GetPrice(travelClass);

                    if (request.MaxPrice.HasValue && price > request.MaxPrice.Value)
                        continue;

                    results.Add(new FlightSearchResult
                    {
                        FlightId = flight.Id,
                        FlightNumber = flight.FlightNumber,
                        DepartureCountry = flight.DepartureCountry,
                        DestinationCountry = flight.DestinationCountry,
                        DepartureAirport = flight.DepartureAirport,
                        ArrivalAirport = flight.ArrivalAirport,
                        DepartureDate = flight.DepartureDate,
                        ArrivalDate = flight.ArrivalDate,
                        TravelClass = travelClass,
                        Price = price,
                        AvailableSeats = availableSeats
                    });
                }
            }

            return results
                .OrderBy(result => result.DepartureDate)
                .ThenBy(result => result.Price)
                .ToList();
        }

        private static int GetAvailableSeats(Flight flight, TravelClass travelClass)
        {
            return travelClass switch
            {
                TravelClass.Economy => flight.EconomySeats,
                TravelClass.Business => flight.BusinessSeats,
                TravelClass.FirstClass => flight.FirstClassSeats,
                _ => 0
            };
        }

        private static bool ContainsIgnoreCase(string source, string value)
        {
            return source.Contains(value.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}

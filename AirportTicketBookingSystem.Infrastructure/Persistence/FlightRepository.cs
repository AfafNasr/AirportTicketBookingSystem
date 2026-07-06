

using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Domain.Entities;

namespace AirportTicketBookingSystem.Infrastructure.Persistence
{
    public sealed class FlightRepository : JsonFileRepository<Flight>, IFlightRepository
    {
        public FlightRepository() : base("Flights.json")
        {
        }

        public async Task<IReadOnlyList<Flight>> GetAllAsync()
        {
            return await ReadAllAsync();
        }

        public async Task<Flight?> GetByIdAsync(Guid id)
        {
            var flights = await ReadAllAsync();

            return flights.FirstOrDefault(flight => flight.Id == id);
        }

        public async Task AddAsync(Flight flight)
        {
            var flights = (await ReadAllAsync()).ToList();

            flights.Add(flight);

            await WriteAllAsync(flights);
        }

        public async Task AddRangeAsync(IEnumerable<Flight> flights)
        {
            var existingFlights = (await ReadAllAsync()).ToList();

            existingFlights.AddRange(flights);

            await WriteAllAsync(existingFlights);
        }

        public async Task UpdateAsync(Flight flight)
        {
            var flights = (await ReadAllAsync()).ToList();

            var index = flights.FindIndex(existingFlight => existingFlight.Id == flight.Id);

            if (index == -1)
                return;

            flights[index] = flight;

            await WriteAllAsync(flights);
        }
    }
}

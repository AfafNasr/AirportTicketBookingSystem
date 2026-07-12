using AirportTicketBookingSystem.Domain.Entities;

namespace AirportTicketBookingSystem.Application.Abstractions.Repositories;

public interface IFlightRepository
{
    Task<IReadOnlyList<Flight>> GetAllAsync();
    Task<Flight?> GetByIdAsync(Guid id);
    Task AddAsync(Flight flight);
    Task AddRangeAsync(IEnumerable<Flight> flights);
    Task UpdateAsync(Flight flight);
}

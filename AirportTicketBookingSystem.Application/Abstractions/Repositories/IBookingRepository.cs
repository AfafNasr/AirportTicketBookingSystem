using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Domain.Entities;

namespace AirportTicketBookingSystem.Application.Abstractions.Repositories
{
    public interface IBookingRepository
    {
        Task<IReadOnlyList<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(Guid id);
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
    }
}

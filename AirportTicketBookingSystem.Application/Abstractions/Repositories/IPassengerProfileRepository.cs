using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Domain.Entities;

namespace AirportTicketBookingSystem.Application.Abstractions.Repositories
{
    public interface IPassengerProfileRepository
    {
        Task<IReadOnlyList<PassengerProfile>> GetAllAsync();
        Task<PassengerProfile?> GetByUserIdAsync(Guid userId);
        Task AddAsync(PassengerProfile passengerProfile);
    }
}

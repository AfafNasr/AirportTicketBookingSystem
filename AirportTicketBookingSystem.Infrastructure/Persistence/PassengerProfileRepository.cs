

using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Domain.Entities;

namespace AirportTicketBookingSystem.Infrastructure.Persistence
{
    public sealed class PassengerProfileRepository
    : JsonFileRepository<PassengerProfile>, IPassengerProfileRepository
    {
        public PassengerProfileRepository() : base(DataFileNames.PassengerProfiles)
        {
        }

        public async Task<IReadOnlyList<PassengerProfile>> GetAllAsync()
        {
            return await ReadAllAsync();
        }

        public async Task<PassengerProfile?> GetByUserIdAsync(Guid userId)
        {
            var profiles = await ReadAllAsync();

            return profiles.FirstOrDefault(profile => profile.UserId == userId);
        }

        public async Task AddAsync(PassengerProfile passengerProfile)
        {
            var profiles = (await ReadAllAsync()).ToList();

            profiles.Add(passengerProfile);

            await WriteAllAsync(profiles);
        }
    }
}

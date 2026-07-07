

using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Domain.Entities;

namespace AirportTicketBookingSystem.Infrastructure.Persistence
{
    public sealed class UserRepository : JsonFileRepository<User>, IUserRepository
    {
        public UserRepository() : base(DataFileNames.Users)
        {
        }

        public async Task<IReadOnlyList<User>> GetAllAsync()
        {
            return await ReadAllAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            var users = await ReadAllAsync();

            return users.FirstOrDefault(user => user.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var users = await ReadAllAsync();

            return users.FirstOrDefault(user =>
                string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task AddAsync(User user)
        {
            var users = (await ReadAllAsync()).ToList();

            users.Add(user);

            await WriteAllAsync(users);
        }

        public async Task UpdateAsync(User user)
        {
            var users = (await ReadAllAsync()).ToList();

            var index = users.FindIndex(existingUser => existingUser.Id == user.Id);

            if (index == -1)
                return;

            users[index] = user;

            await WriteAllAsync(users);
        }
    }
}

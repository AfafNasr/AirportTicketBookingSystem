using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Application.Abstractions.Services;
using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Infrastructure.Seed
{
    public sealed class ManagerSeeder
    {
        private const string ManagerEmail = "admin@airport.com";
        private const string ManagerPassword = "Admin@123";

        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public ManagerSeeder(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task SeedAsync()
        {
            var existingManager = await _userRepository.GetByEmailAsync(ManagerEmail);

            if (existingManager is not null)
                return;

            var manager = new User(
                "System Manager",
                ManagerEmail,
                _passwordHasher.Hash(ManagerPassword),
                UserRole.Manager);

            await _userRepository.AddAsync(manager);
        }
    }
}

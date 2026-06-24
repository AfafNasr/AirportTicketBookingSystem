using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Application.Abstractions.Services;
using AirportTicketBookingSystem.Application.Common;
using AirportTicketBookingSystem.Application.DTOs.Auth;
using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Application.Services
{
    public sealed class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPassengerProfileRepository _passengerProfileRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICurrentUserService _currentUserService;

        public AuthService(
            IUserRepository userRepository,
            IPassengerProfileRepository passengerProfileRepository,
            IPasswordHasher passwordHasher,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _passengerProfileRepository = passengerProfileRepository;
            _passwordHasher = passwordHasher;
            _currentUserService = currentUserService;
        }

        public async Task<Result<AuthResponse>> RegisterPassengerAsync(RegisterPassengerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                return Result<AuthResponse>.Failure("Full name is required.");

            if (string.IsNullOrWhiteSpace(request.Email))
                return Result<AuthResponse>.Failure("Email is required.");

            if (string.IsNullOrWhiteSpace(request.Password))
                return Result<AuthResponse>.Failure("Password is required.");

            if (string.IsNullOrWhiteSpace(request.PassportNumber))
                return Result<AuthResponse>.Failure("Passport number is required.");

            var existingUser = await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser is not null)
                return Result<AuthResponse>.Failure("Email is already registered.");

            var passwordHash = _passwordHasher.Hash(request.Password);

            var user = new User(
                request.FullName.Trim(),
                request.Email.Trim(),
                passwordHash,
                UserRole.Passenger);

            var passengerProfile = new PassengerProfile(
                user.Id,
                request.PassportNumber.Trim());

            await _userRepository.AddAsync(user);
            await _passengerProfileRepository.AddAsync(passengerProfile);

            _currentUserService.SignIn(user.Id, user.Email, user.Role);

            return Result<AuthResponse>.Success(new AuthResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            });
        }

        public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return Result<AuthResponse>.Failure("Email is required.");

            if (string.IsNullOrWhiteSpace(request.Password))
                return Result<AuthResponse>.Failure("Password is required.");

            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user is null)
                return Result<AuthResponse>.Failure("Invalid email or password.");

            var isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
                return Result<AuthResponse>.Failure("Invalid email or password.");

            _currentUserService.SignIn(user.Id, user.Email, user.Role);

            return Result<AuthResponse>.Success(new AuthResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            });
        }

        public void Logout()
        {
            _currentUserService.SignOut();
        }
    }
}

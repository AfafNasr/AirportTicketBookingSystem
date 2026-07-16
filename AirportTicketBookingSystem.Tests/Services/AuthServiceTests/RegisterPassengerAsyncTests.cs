using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Application.Abstractions.Services;
using AirportTicketBookingSystem.Application.DTOs.Auth;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Domain.Enums;
using Moq;

namespace AirportTicketBookingSystem.Tests.Services.AuthServiceTests;

public  class RegisterPassengerAsyncTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPassengerProfileRepository> _passengerProfileRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ICurrentUserSession> _currentUserSessionMock;

    private readonly AuthService _authService;

    public RegisterPassengerAsyncTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passengerProfileRepositoryMock = new Mock<IPassengerProfileRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _currentUserSessionMock = new Mock<ICurrentUserSession>();
        _authService = new AuthService(
            _userRepositoryMock.Object,
            _passengerProfileRepositoryMock.Object,
            _passwordHasherMock.Object,
            _currentUserSessionMock.Object);
    }

    [Fact]
    public async Task RegisterPassengerAsync_WhenFullNameIsEmpty_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterPassengerRequest
        {
            FullName = string.Empty,
            Email = "passenger@example.com",
            Password = "Password123!",
            PassportNumber = "P123456"
        };

        // Act
        var result = await _authService.RegisterPassengerAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Full name is required.", result.Error);
    }

    [Fact]
    public async Task RegisterPassengerAsync_WhenEmailIsEmpty_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterPassengerRequest
        {
            FullName = "Passenger1",
            Email = string.Empty,
            Password = "Password123!",
            PassportNumber = "P123456"
        };

        // Act
        var result = await _authService.RegisterPassengerAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Email is required.", result.Error);
    }

    [Fact]
    public async Task RegisterPassengerAsync_WhenPasswordIsEmpty_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterPassengerRequest
        {
            FullName = "Passenger1",
            Email = "passenger@example.com",
            Password = string.Empty,
            PassportNumber = "P123456"
        };

        // Act
        var result = await _authService.RegisterPassengerAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Password is required.", result.Error);
    }

    [Fact]
    public async Task RegisterPassengerAsync_WhenPassportNumberIsEmpty_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterPassengerRequest
        {
            FullName = "Passenger1",
            Email = "passenger@example.com",
            Password = "Password123!",
            PassportNumber = string.Empty
        };

        // Act
        var result = await _authService.RegisterPassengerAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Passport number is required.", result.Error);
    }

    [Fact]
    public async Task RegisterPassengerAsync_WhenEmailAlreadyRegistered_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterPassengerRequest
        {
            FullName = "Passenger1",
            Email = "passenger@example.com",
            Password = "Password123!",
            PassportNumber = "P123456"
        };

        var existingUser = new User(
            "Existing Passenger",
            request.Email,
            "hashed-password",
            UserRole.Passenger);

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _authService.RegisterPassengerAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Email is already registered.", result.Error);

    }

    [Fact]
    public async Task RegisterPassengerAsync_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterPassengerRequest
        {
            FullName = "Passenger One",
            Email = "passenger@example.com",
            Password = "Password123!",
            PassportNumber = "P123456"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.Hash(request.Password))
            .Returns("hashed-password");

        // Act
        var result = await _authService.RegisterPassengerAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        
    }

}

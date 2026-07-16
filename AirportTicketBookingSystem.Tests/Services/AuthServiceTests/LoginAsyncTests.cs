using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Application.Abstractions.Services;
using AirportTicketBookingSystem.Application.DTOs.Auth;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Domain.Enums;
using Moq;

namespace AirportTicketBookingSystem.Tests.Services.AuthServiceTests;

public class LoginAsyncTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPassengerProfileRepository> _passengerProfileRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ICurrentUserSession> _currentUserSessionMock;

    private readonly AuthService _authService;

    public LoginAsyncTests()
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
    public async Task LoginAsync_WhenEmailIsEmpty_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = string.Empty,
            Password = "Password123!"
           
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Email is required.", result.Error);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsEmpty_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "passenger@example.com",
            Password = string.Empty
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Password is required.", result.Error);
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "passenger@example.com",
            Password = "Password123!"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid email or password.", result.Error);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordNotValid_ReturnsFailure()
    {
        var request = new LoginRequest
        {
            Email = "passenger@example.com",
            Password = "Password123!"
        };

        var user = new User(
            "Passenger One",
            request.Email,
            "hashed-password",
            UserRole.Passenger);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid email or password.", result.Error);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsSuccess()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "passenger@example.com",
            Password = "Password123!"
        };

        var user = new User(
            "Passenger One",
            request.Email,
            "hashed-password",
            UserRole.Passenger);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
    }
}

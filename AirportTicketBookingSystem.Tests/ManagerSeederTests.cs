using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Application.Abstractions.Services;
using AirportTicketBookingSystem.Domain.Entities;
using AirportTicketBookingSystem.Domain.Enums;
using AirportTicketBookingSystem.Infrastructure.Seed;
using Moq;

namespace AirportTicketBookingSystem.Tests;

public class ManagerSeederTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;

    private readonly ManagerSeeder _managerSeeder;

    public ManagerSeederTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();

        _managerSeeder = new ManagerSeeder(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task SeedAsync_WhenManagerAlreadyExists_DoesNotAddManager()
    {
        // Arrange
        var existingManager = new User(
            "System Manager",
            "admin@airport.com",
            "hashed-password",
            UserRole.Manager);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("admin@airport.com"))
            .ReturnsAsync(existingManager);

        // Act
        await _managerSeeder.SeedAsync();

        // Assert
        _passwordHasherMock.Verify(
            x => x.Hash(It.IsAny<string>()),
            Times.Never);

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task SeedAsync_WhenManagerDoesNotExist_AddsManager()
    {
        // Arrange
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("admin@airport.com"))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.Hash("Admin@123"))
            .Returns("hashed-password");

        // Act
        await _managerSeeder.SeedAsync();

        // Assert
        _passwordHasherMock.Verify(
            x => x.Hash("Admin@123"),
            Times.Once);

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.Is<User>(user =>
                user.FullName == "System Manager" &&
                user.Email == "admin@airport.com" &&
                user.PasswordHash == "hashed-password" &&
                user.Role == UserRole.Manager)),
            Times.Once);
    }

}

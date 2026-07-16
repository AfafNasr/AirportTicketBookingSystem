using AirportTicketBookingSystem.Infrastructure.Security;

namespace AirportTicketBookingSystem.Tests.Infrastructure.Security;

public class Sha256PasswordHasherTests
{
    private readonly Sha256PasswordHasher _hasher;

    public Sha256PasswordHasherTests()
    {
        _hasher = new Sha256PasswordHasher();
    }

    [Fact]
    public void Hash_WhenPasswordIsValid_ReturnsHashedPassword()
    {
        // Arrange
        const string password = "Password123!";

        // Act
        var hash = _hasher.Hash(password);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void Hash_WhenSamePasswordProvided_ReturnsSameHash()
    {
        // Arrange
        const string password = "Password123!";

        // Act
        var firstHash = _hasher.Hash(password);
        var secondHash = _hasher.Hash(password);

        // Assert
        Assert.Equal(firstHash, secondHash);
    }

    [Fact]
    public void Verify_WhenPasswordMatchesHash_ReturnsTrue()
    {
        // Arrange
        const string password = "Password123!";
        var hash = _hasher.Hash(password);

        // Act
        var result = _hasher.Verify(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_WhenPasswordDoesNotMatchHash_ReturnsFalse()
    {
        // Arrange
        var hash = _hasher.Hash("Password123!");

        // Act
        var result = _hasher.Verify("WrongPassword", hash);

        // Assert
        Assert.False(result);
    }

  
   
}
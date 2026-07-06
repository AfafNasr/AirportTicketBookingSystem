using AirportTicketBookingSystem.Application.Abstractions.Services;

using System.Security.Cryptography;
using System.Text;

namespace AirportTicketBookingSystem.Infrastructure.Security
{
    public sealed class Sha256PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = SHA256.HashData(bytes);

            return Convert.ToHexString(hashBytes);
        }

        public bool Verify(string password, string passwordHash)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
            ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

            var computedHash = Hash(password);

            return string.Equals(computedHash, passwordHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}

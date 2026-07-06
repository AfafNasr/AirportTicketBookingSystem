

using AirportTicketBookingSystem.Application.Abstractions.Services;
using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.ConsoleApp.Session
{
    public sealed class CurrentUserService : ICurrentUserService
    {
        public Guid? UserId { get; private set; }
        public string? Email { get; private set; }
        public UserRole? Role { get; private set; }

        public bool IsAuthenticated => UserId.HasValue;

        public void SignIn(Guid userId, string email, UserRole role)
        {
            UserId = userId;
            Email = email;
            Role = role;
        }

        public void SignOut()
        {
            UserId = null;
            Email = null;
            Role = null;
        }
    }
}

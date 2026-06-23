using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Application.Abstractions.Services
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? Email { get; }
        UserRole? Role { get; }
        bool IsAuthenticated { get; }

        void SignIn(Guid userId, string email, UserRole role);
        void SignOut();
    }
}

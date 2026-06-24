using AirportTicketBookingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirportTicketBookingSystem.Application.DTOs.Auth
{
    public sealed class AuthResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}

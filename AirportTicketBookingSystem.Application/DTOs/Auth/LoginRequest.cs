using System;
using System.Collections.Generic;
using System.Text;

namespace AirportTicketBookingSystem.Application.DTOs.Auth
{
    public sealed class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

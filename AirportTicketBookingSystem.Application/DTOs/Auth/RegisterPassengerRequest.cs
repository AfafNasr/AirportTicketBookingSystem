using System;
using System.Collections.Generic;
using System.Text;

namespace AirportTicketBookingSystem.Application.DTOs.Auth
{
    public sealed class RegisterPassengerRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Domain.Common;
using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.Domain.Entities
{
    public sealed class User : Entity
    {
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }

        private User()
        {
            FullName = string.Empty;
            Email = string.Empty;
            PasswordHash = string.Empty;
        }

        public User(string fullName, string email, string passwordHash, UserRole role)
        {
            FullName = fullName;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
        }
    }
}

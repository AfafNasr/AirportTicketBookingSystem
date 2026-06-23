using System;
using System.Collections.Generic;
using System.Text;

namespace AirportTicketBookingSystem.Application.Abstractions.Services
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string passwordHash);
    }
}

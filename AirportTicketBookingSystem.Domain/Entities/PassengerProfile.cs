using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Domain.Common;

namespace AirportTicketBookingSystem.Domain.Entities
{
    public sealed class PassengerProfile : Entity
    {
        public Guid UserId { get;  set; }
        public string PassportNumber { get; private set; }

        private PassengerProfile()
        {
            PassportNumber = string.Empty;
        }

        public PassengerProfile(Guid userId, string passportNumber)
        {
            UserId = userId;
            PassportNumber = passportNumber;
        }
    }
}

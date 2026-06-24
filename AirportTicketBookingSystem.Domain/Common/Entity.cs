using System;
using System.Collections.Generic;
using System.Text;

namespace AirportTicketBookingSystem.Domain.Common
{
    public abstract class Entity
    {
        public Guid Id { get;  set; } = Guid.NewGuid();
    }
}

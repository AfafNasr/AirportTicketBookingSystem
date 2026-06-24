using System;
using System.Collections.Generic;
using System.Text;

namespace AirportTicketBookingSystem.Application.DTOs.Flights
{
    public sealed class FlightImportError
    {
        public int RowNumber { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}



namespace AirportTicketBookingSystem.Application.DTOs.Flights
{
    public sealed class ValidationRuleDetail
    {
        public string FieldName { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public List<string> Constraints { get; set; } = [];
    }
}

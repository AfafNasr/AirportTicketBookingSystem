namespace AirportTicketBookingSystem.Application.DTOs.Flights;

public sealed class FlightImportResult
{
    public int ImportedCount { get; set; }
    public List<FlightImportError> Errors { get; set; } = [];

    public bool HasErrors => Errors.Count > 0;
}

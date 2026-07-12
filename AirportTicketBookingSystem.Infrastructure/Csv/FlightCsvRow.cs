namespace AirportTicketBookingSystem.Infrastructure.Csv;

public sealed class FlightCsvRow
{
    public int RowNumber { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string DepartureCountry { get; set; } = string.Empty;
    public string DestinationCountry { get; set; } = string.Empty;
    public string DepartureAirport { get; set; } = string.Empty;
    public string ArrivalAirport { get; set; } = string.Empty;
    public string DepartureDate { get; set; } = string.Empty;
    public string ArrivalDate { get; set; } = string.Empty;
    public string EconomyPrice { get; set; } = string.Empty;
    public string BusinessPrice { get; set; } = string.Empty;
    public string FirstClassPrice { get; set; } = string.Empty;
    public string EconomySeats { get; set; } = string.Empty;
    public string BusinessSeats { get; set; } = string.Empty;
    public string FirstClassSeats { get; set; } = string.Empty;
}

using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Input;

namespace AirportTicketBookingSystem.ConsoleApp.Handlers.Manager;

public sealed class ManagerValidationRulesHandler
{
    private readonly FlightValidationMetadataService _validationMetadataService;

    public ManagerValidationRulesHandler(FlightValidationMetadataService validationMetadataService)
    {
        _validationMetadataService = validationMetadataService;
    }

    public void ViewValidationRules()
    {
        ConsoleUi.Header("FLIGHT VALIDATION RULES");

        var rules = _validationMetadataService.GetFlightValidationRules();

        foreach (var rule in rules)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(rule.FieldName);
            Console.ResetColor();

            Console.WriteLine($"Type: {rule.FieldType}");
            Console.WriteLine("Constraints:");

            foreach (var constraint in rule.Constraints)
            {
                Console.WriteLine($" - {constraint}");
            }

            Console.WriteLine();
        }

        ConsoleUi.Pause();
    }
}

using System;
using System.Collections.Generic;
using System.Text;

using System.Reflection;
using System.Text.RegularExpressions;
using AirportTicketBookingSystem.Application.DTOs.Flights;
using AirportTicketBookingSystem.Domain.Entities;

namespace AirportTicketBookingSystem.Application.Services
{
    public sealed class FlightValidationMetadataService
    {
        private static readonly Dictionary<string, string[]> ConstraintsByPropertyName = new()
        {
            ["FlightNumber"] =
            [
                "Required",
            "Must be unique with Departure Date"
            ],

            ["DepartureCountry"] =
            [
                "Required"
            ],

            ["DestinationCountry"] =
            [
                "Required"
            ],

            ["DepartureDate"] =
            [
                "Required",
            "Allowed Range: Today → Future"
            ],

            ["DepartureAirport"] =
            [
                "Required"
            ],

            ["ArrivalAirport"] =
            [
                "Required"
            ],

            ["ArrivalDate"] =
            [
                "Required",
            "Must be after Departure Date"
            ],

            ["EconomyPrice"] =
            [
                "Required",
            "Minimum Value: 0"
            ],

            ["BusinessPrice"] =
            [
                "Required",
            "Minimum Value: 0"
            ],

            ["FirstClassPrice"] =
            [
                "Required",
            "Minimum Value: 0"
            ],

            ["EconomySeats"] =
            [
                "Required",
            "Minimum Value: 0"
            ],

            ["BusinessSeats"] =
            [
                "Required",
            "Minimum Value: 0"
            ],

            ["FirstClassSeats"] =
            [
                "Required",
            "Minimum Value: 0"
            ]
        };

        public IReadOnlyList<ValidationRuleDetail> GetFlightValidationRules()
        {
            return typeof(Flight)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.Name != "Id") 
                .Select(property => {
                    var constraints = new List<string>();

                   
                    if (property.PropertyType == typeof(int) || property.PropertyType == typeof(decimal))
                    {
                        constraints.Add("Required");
                        constraints.Add("Minimum Value: 0");
                    }
                    else if (property.PropertyType == typeof(string) || property.PropertyType == typeof(DateTime))
                    {
                        constraints.Add("Required");
                    }

                
                    if (ConstraintsByPropertyName.TryGetValue(property.Name, out var advancedConstraints))
                    {
                        foreach (var constraint in advancedConstraints)
                        {
                            if (!constraints.Contains(constraint)) constraints.Add(constraint);
                        }
                    }

                    return new ValidationRuleDetail
                    {
                        FieldName = SplitCamelCase(property.Name),
                        FieldType = MapToFriendlyTypeName(property.PropertyType),
                        Constraints = constraints
                    };
                })
                .ToList();
        }

        private static string SplitCamelCase(string value)
        {
            return Regex.Replace(value, "([A-Z])", " $1").Trim();
        }

        private static string MapToFriendlyTypeName(Type type)
        {
            var actualType = Nullable.GetUnderlyingType(type) ?? type;

            if (actualType == typeof(string))
                return "Free Text";

            if (actualType == typeof(DateTime))
                return "Date Time";

            if (actualType == typeof(decimal))
                return "Decimal";

            if (actualType == typeof(int))
                return "Integer";

            return actualType.Name;
        }
    }
}

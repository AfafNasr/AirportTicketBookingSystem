using AirportTicketBookingSystem.Application.Services;

namespace AirportTicketBookingSystem.Tests.Services.FlightValidationMetadataServiceTests;

public class GetFlightValidationRulesTests
{
    private readonly FlightValidationMetadataService _service;

    public GetFlightValidationRulesTests()
    {
        _service = new FlightValidationMetadataService();
    }
    [Fact]
    public void GetFlightValidationRules_ReturnsRulesForAllFlightPropertiesExceptId()
    {
        // Act
        var result = _service.GetFlightValidationRules();

        // Assert
        Assert.Equal(13, result.Count);
        Assert.DoesNotContain(result, rule => rule.FieldName == "Id");
    }
    [Fact]
    public void GetFlightValidationRules_FormatsFieldNamesCorrectly()
    {
        // Act
        var result = _service.GetFlightValidationRules();

        // Assert
        Assert.Contains(
            result,
            rule => rule.FieldName == "Flight Number");

        Assert.Contains(
            result,
            rule => rule.FieldName == "Departure Country");

        Assert.Contains(
            result,
            rule => rule.FieldName == "First Class Price");
    }
    [Fact]
    public void GetFlightValidationRules_ReturnsFriendlyFieldTypes()
    {
        // Act
        var result = _service.GetFlightValidationRules();

        // Assert
        var flightNumberRule =
            result.First(rule => rule.FieldName == "Flight Number");

        var departureDateRule =
            result.First(rule => rule.FieldName == "Departure Date");

        var economyPriceRule =
            result.First(rule => rule.FieldName == "Economy Price");

        var economySeatsRule =
            result.First(rule => rule.FieldName == "Economy Seats");

        Assert.Equal("Free Text", flightNumberRule.FieldType);
        Assert.Equal("Date Time", departureDateRule.FieldType);
        Assert.Equal("Decimal", economyPriceRule.FieldType);
        Assert.Equal("Integer", economySeatsRule.FieldType);
    }
    [Fact]
    public void GetFlightValidationRules_StringFieldsContainRequiredConstraint()
    {
        // Act
        var result = _service.GetFlightValidationRules();

        // Assert
        var departureCountryRule =
            result.First(rule => rule.FieldName == "Departure Country");

        Assert.Contains(
            "Required",
            departureCountryRule.Constraints);
    }
    [Theory]
    [InlineData("Economy Price")]
    [InlineData("Business Price")]
    [InlineData("First Class Price")]
    [InlineData("Economy Seats")]
    [InlineData("Business Seats")]
    [InlineData("First Class Seats")]
    public void GetFlightValidationRules_NumericFieldsContainMinimumValueConstraint(
    string fieldName)
    {
        // Act
        var result = _service.GetFlightValidationRules();

        // Assert
        var rule = result.First(item => item.FieldName == fieldName);

        Assert.Contains("Required", rule.Constraints);
        Assert.Contains("Minimum Value: 0", rule.Constraints);
    }
    [Fact]
    public void GetFlightValidationRules_FlightNumberContainsUniqueConstraint()
    {
        // Act
        var result = _service.GetFlightValidationRules();

        // Assert
        var rule =
            result.First(item => item.FieldName == "Flight Number");

        Assert.Contains(
            "Must be unique with Departure Date",
            rule.Constraints);
    }
    [Fact]
    public void GetFlightValidationRules_DepartureDateContainsAllowedRangeConstraint()
    {
        // Act
        var result = _service.GetFlightValidationRules();

        // Assert
        var rule =
            result.First(item => item.FieldName == "Departure Date");

        Assert.Contains(
            "Allowed Range: Today → Future",
            rule.Constraints);
    }
    [Fact]
    public void GetFlightValidationRules_ArrivalDateContainsAfterDepartureConstraint()
    {
        // Act
        var result = _service.GetFlightValidationRules();

        // Assert
        var rule =
            result.First(item => item.FieldName == "Arrival Date");

        Assert.Contains(
            "Must be after Departure Date",
            rule.Constraints);
    }
}

using AirportTicketBookingSystem.Application.Helpers;

namespace AirportTicketBookingSystem.Tests.Helpers;

public class CsvRowParserTests
{
    [Fact]
    public void ParseLine_WhenLineContainsSimpleValues_ReturnsColumns()
    {
        // Arrange
        const string line = "RJ101,Jordan,UAE";

        // Act
        var result = CsvRowParser.ParseLine(line);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("RJ101", result[0]);
        Assert.Equal("Jordan", result[1]);
        Assert.Equal("UAE", result[2]);
    }
    [Fact]
    public void ParseLine_WhenValuesContainSpaces_TrimsValues()
    {
        // Arrange
        const string line = " RJ101 , Jordan , UAE ";

        // Act
        var result = CsvRowParser.ParseLine(line);

        // Assert
        Assert.Equal("RJ101", result[0]);
        Assert.Equal("Jordan", result[1]);
        Assert.Equal("UAE", result[2]);
    }

    [Fact]
    public void ParseLine_WhenQuotedValueContainsComma_DoesNotSplitInsideQuotes()
    {
        // Arrange
        const string line = "RJ101,\"Amman, Jordan\",UAE";

        // Act
        var result = CsvRowParser.ParseLine(line);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("RJ101", result[0]);
        Assert.Equal("Amman, Jordan", result[1]);
        Assert.Equal("UAE", result[2]);
    }

    [Fact]
    public void ParseLine_WhenQuotedValueContainsEscapedQuotes_ReturnsSingleQuotes()
    {
        // Arrange
        const string line = "RJ101,\"Queen \"\"Alia\"\" Airport\",UAE";

        // Act
        var result = CsvRowParser.ParseLine(line);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Queen \"Alia\" Airport", result[1]);
    }

    [Fact]
    public void ParseLine_WhenColumnIsEmpty_ReturnsEmptyValue()
    {
        // Arrange
        const string line = "RJ101,,UAE";

        // Act
        var result = CsvRowParser.ParseLine(line);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(string.Empty, result[1]);
    }

    [Fact]
    public void ParseLine_WhenLineEndsWithComma_ReturnsEmptyLastColumn()
    {
        // Arrange
        const string line = "RJ101,Jordan,";

        // Act
        var result = CsvRowParser.ParseLine(line);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(string.Empty, result[2]);
    }

    [Fact]
    public void ParseLine_WhenLineIsEmpty_ReturnsSingleEmptyColumn()
    {
        // Arrange
        const string line = "";

        // Act
        var result = CsvRowParser.ParseLine(line);

        // Assert
        Assert.Single(result);
        Assert.Equal(string.Empty, result[0]);
    }

    [Fact]
    public void ParseLine_WhenEntireValueIsQuoted_ReturnsValueWithoutQuotes()
    {
        // Arrange
        const string line = "\"RJ101\",\"Jordan\",\"UAE\"";

        // Act
        var result = CsvRowParser.ParseLine(line);

        // Assert
        Assert.Equal("RJ101", result[0]);
        Assert.Equal("Jordan", result[1]);
        Assert.Equal("UAE", result[2]);
    }
}

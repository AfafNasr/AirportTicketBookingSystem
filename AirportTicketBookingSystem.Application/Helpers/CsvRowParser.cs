using System.Text;

namespace AirportTicketBookingSystem.Application.Helpers
{
    public static class CsvRowParser
    {
        public static IReadOnlyList<string> ParseLine(string line)
        {
            var columns = new List<string>();
            var currentColumn = new StringBuilder();
            var insideQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var currentChar = line[i];

                if (currentChar == '"')
                {
                    if (insideQuotes &&
                        i + 1 < line.Length &&
                        line[i + 1] == '"')
                    {
                        currentColumn.Append('"');
                        i++;
                        continue;
                    }

                    insideQuotes = !insideQuotes;
                    continue;
                }

                if (currentChar == ',' && !insideQuotes)
                {
                    columns.Add(currentColumn.ToString().Trim());
                    currentColumn.Clear();
                    continue;
                }

                currentColumn.Append(currentChar);
            }

            columns.Add(currentColumn.ToString().Trim());

            return columns;
        }
    }
}

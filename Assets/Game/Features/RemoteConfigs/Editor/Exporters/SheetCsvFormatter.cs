using System.Collections.Generic;
using System.Text;

namespace Features.RemoteConfigs.Editor.Exporters
{
    public class SheetCsvFormatter
    {
        private const string LineSeparator = "\r\n";

        private readonly char[] _charactersToQuote = { ',', '"', '\r', '\n' };

        public string ToCsv(IReadOnlyList<List<string>> values)
        {
            var builder = new StringBuilder();

            foreach (var line in values)
            {
                for (var index = 0; index < line.Count; index++)
                {
                    if (index > 0)
                    {
                        builder.Append(',');
                    }

                    builder.Append(Escape(line[index]));
                }

                builder.Append(LineSeparator);
            }

            return builder.ToString();
        }

        private string Escape(string value)
        {
            if (value.IndexOfAny(_charactersToQuote) < 0)
            {
                return value;
            }

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}

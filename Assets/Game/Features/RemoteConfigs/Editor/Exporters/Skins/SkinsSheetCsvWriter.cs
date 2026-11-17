using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Features.RemoteConfigs.Editor.Models;

namespace Features.RemoteConfigs.Editor.Exporters.Skins
{
    public class SkinsSheetCsvWriter
    {
        private const string PriceFormat = "0.00";
        private const string LineSeparator = "\r\n";
        private const string StatTrakColumn = "AvailableStatTrak";
        private const string SouvenirColumn = "AvailableSouvenir";

        public ICollection<int> BooleanColumns => _booleanColumns;
        public int ColumnsCount => _header.Count;

        private readonly CultureInfo _priceCulture;
        private readonly char[] _charactersToQuote;

        private readonly List<string> _header = new List<string>
        {
            "Id",
            "Name",
            "Rarity",
            "Price",
            StatTrakColumn,
            SouvenirColumn,
        };

        private readonly HashSet<int> _booleanColumns;

        public SkinsSheetCsvWriter()
        {
            _priceCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            _priceCulture.NumberFormat.NumberDecimalSeparator = ",";
            _charactersToQuote = new[] { ',', '"', '\r', '\n' };
            _booleanColumns = new HashSet<int> { _header.IndexOf(StatTrakColumn), _header.IndexOf(SouvenirColumn) };
        }

        public List<List<string>> BuildValues(IReadOnlyList<SkinDataModel> rows)
        {
            var values = new List<List<string>>(rows.Count + 1)
            {
                new List<string>(_header),
            };

            foreach (var row in rows)
            {
                values.Add(new List<string>
                {
                    row.Id,
                    row.Name,
                    row.Rarity.ToString(),
                    FormatPrice(row.Price.From, row.Price.To),
                    FormatBool(row.AvailableStatTrak),
                    FormatBool(row.AvailableSouvenir),
                });
            }

            return values;
        }

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

        private string FormatPrice(float from, float to)
        {
            return $"{from.ToString(PriceFormat, _priceCulture)}-{to.ToString(PriceFormat, _priceCulture)}";
        }

        private string FormatBool(bool value)
        {
            if (value == true)
            {
                return SheetBooleanValues.True;
            }

            return SheetBooleanValues.False;
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

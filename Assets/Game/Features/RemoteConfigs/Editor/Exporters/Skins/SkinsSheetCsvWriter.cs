using System.Collections.Generic;
using System.Globalization;
using Features.RemoteConfigs.Editor.Models;

namespace Features.RemoteConfigs.Editor.Exporters.Skins
{
    public class SkinsSheetCsvWriter
    {
        private const string PriceFormat = "0.00";
        private const string StatTrakColumn = "AvailableStatTrak";
        private const string SouvenirColumn = "AvailableSouvenir";

        public ICollection<int> BooleanColumns => _booleanColumns;
        public int ColumnsCount => _header.Count;

        private readonly CultureInfo _priceCulture;
        private readonly SheetCsvFormatter _csvFormatter = new SheetCsvFormatter();

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
            return _csvFormatter.ToCsv(values);
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
    }
}

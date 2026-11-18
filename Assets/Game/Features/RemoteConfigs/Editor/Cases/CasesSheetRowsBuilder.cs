using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CasesSheetRowsBuilder
    {
        private const string PriceFormat = "0.00";
        private const string WeightFormat = "R";

        public int ColumnsCount => _header.Count;

        private readonly List<string> _header = new List<string> { "Id", "Name", "Price", "SkinId", "Weight" };
        private readonly CultureInfo _numberCulture;

        public CasesSheetRowsBuilder()
        {
            _numberCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            _numberCulture.NumberFormat.NumberDecimalSeparator = ",";
        }

        public List<List<string>> BuildValues(IReadOnlyList<CasesSheetCase> cases)
        {
            EnsureUniqueIds(cases);

            var values = new List<List<string>> { new List<string>(_header) };

            foreach (var sheetCase in cases)
            {
                var price = sheetCase.Price.ToString(PriceFormat, _numberCulture);

                if (sheetCase.Skins.Count == 0)
                {
                    values.Add(new List<string> { sheetCase.Id, sheetCase.Name, price, string.Empty, string.Empty });
                    continue;
                }

                for (var index = 0; index < sheetCase.Skins.Count; index++)
                {
                    var skin = sheetCase.Skins[index];
                    var weight = skin.Value.ToString(WeightFormat, _numberCulture);

                    if (index == 0)
                    {
                        values.Add(new List<string> { sheetCase.Id, sheetCase.Name, price, skin.Key, weight });
                    }
                    else
                    {
                        values.Add(new List<string> { string.Empty, string.Empty, string.Empty, skin.Key, weight });
                    }
                }
            }

            return values;
        }

        private void EnsureUniqueIds(IReadOnlyList<CasesSheetCase> cases)
        {
            var emptyIdsCount = cases.Count(sheetCase => string.IsNullOrWhiteSpace(sheetCase.Id) == true);

            if (emptyIdsCount > 0)
            {
                throw new InvalidOperationException($"{emptyIdsCount} case(s) have an empty Id");
            }

            var duplicates = cases.GroupBy(sheetCase => sheetCase.Id)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicates.Count > 0)
            {
                throw new InvalidOperationException($"Duplicate case ids: {string.Join(", ", duplicates)}");
            }
        }
    }
}

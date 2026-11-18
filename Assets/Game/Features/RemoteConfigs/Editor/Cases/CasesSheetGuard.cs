using System;
using System.Collections.Generic;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CasesSheetGuard
    {
        private const int HeaderRowsCount = 1;
        private const int IdColumn = 0;

        public List<string> FindUnknownIds(IReadOnlyList<List<string>> sheetValues, ICollection<string> projectIds)
        {
            var unknownIds = new List<string>();
            var seenIds = new HashSet<string>(StringComparer.Ordinal);

            for (var index = HeaderRowsCount; index < sheetValues.Count; index++)
            {
                var row = sheetValues[index];

                if (row.Count <= IdColumn || string.IsNullOrWhiteSpace(row[IdColumn]) == true)
                {
                    continue;
                }

                var id = row[IdColumn].Trim();

                if (projectIds.Contains(id) == false && seenIds.Add(id) == true)
                {
                    unknownIds.Add(id);
                }
            }

            return unknownIds;
        }
    }
}

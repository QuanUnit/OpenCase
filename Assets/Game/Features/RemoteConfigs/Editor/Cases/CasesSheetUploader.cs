using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Features.RemoteConfigs.Editor.GoogleSheets;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CasesSheetUploader
    {
        private readonly GoogleSheetsClient _client;
        private readonly CasesSheetRowsBuilder _rowsBuilder = new CasesSheetRowsBuilder();
        private readonly CasesSheetGuard _guard = new CasesSheetGuard();

        public CasesSheetUploader(GoogleSheetsClient client)
        {
            _client = client;
        }

        public async Task<int> Upload(string keyPath, string spreadsheetId, string pageId, IReadOnlyList<CasesSheetCase> cases, Func<IReadOnlyList<string>, bool> confirmUnknownIds)
        {
            var values = _rowsBuilder.BuildValues(cases);
            var projectIds = new HashSet<string>(cases.Select(sheetCase => sheetCase.Id), StringComparer.Ordinal);

            var session = await _client.Connect(keyPath, spreadsheetId, pageId);
            var existingValues = await _client.ReadValues(session, _rowsBuilder.ColumnsCount);
            var unknownIds = _guard.FindUnknownIds(existingValues, projectIds);

            if (unknownIds.Count > 0 && confirmUnknownIds(unknownIds) == false)
            {
                return 0;
            }

            return await _client.WriteValues(session, values, new HashSet<int>());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Features.RemoteConfigs.Editor.Models;
using Features.RemoteConfigs.Editor.Parsers;
using Shared.Tools.Web;

namespace Features.RemoteConfigs.Editor.Exporters.Skins
{
    public class SkinsSheetExporter
    {
        private const int HeaderRowsCount = 1;
        private const int IdColumn = 0;

        public int ColumnsCount => _csvWriter.ColumnsCount;

        private readonly SkinsSheetExportConfig _config;
        private readonly SkinsSourcesParser _sourcesParser;
        private readonly SkinsSheetParser _sheetParser;
        private readonly SkinsSheetMerger _merger;
        private readonly SkinsSheetCsvWriter _csvWriter;

        public SkinsSheetExporter(SkinsSheetExportConfig config)
        {
            _config = config;
            _sourcesParser = new SkinsSourcesParser();
            _sheetParser = new SkinsSheetParser();
            _merger = new SkinsSheetMerger();
            _csvWriter = new SkinsSheetCsvWriter();
        }

        public async Task<SkinsSheetExportResult> BuildFromPublicCsv(Action<string, float> progressChanged)
        {
            var skinsJson = await DownloadSkins(progressChanged);
            var pricesJson = await DownloadPrices(progressChanged);

            progressChanged("Downloading current sheet", 0f);
            var sheetCsv = await DownloadTools.DownloadGoogleSheetAsCsv(_config.SheetId, _config.PageId);

            if (sheetCsv == null)
            {
                throw new InvalidOperationException("Current skins sheet could not be downloaded");
            }

            progressChanged("Building rows", 1f);
            return Build(skinsJson, pricesJson, sheetCsv);
        }

        public async Task<SkinsSheetExportResult> Build(Action<string, float> progressChanged, IReadOnlyList<List<string>> existingValues)
        {
            var skinsJson = await DownloadSkins(progressChanged);
            var pricesJson = await DownloadPrices(progressChanged);

            progressChanged("Building rows", 1f);
            return Build(skinsJson, pricesJson, existingValues);
        }

        public SkinsSheetExportResult Build(string skinsJson, string pricesJson, string sheetCsv)
        {
            return Build(skinsJson, pricesJson, ReadCsvValues(sheetCsv));
        }

        public SkinsSheetExportResult Build(string skinsJson, string pricesJson, IReadOnlyList<List<string>> existingValues)
        {
            var existingRowsCount = GetLastFilledRowIndex(existingValues);
            EnsureNoEmptyIds(existingValues, existingRowsCount);

            var normalizedValues = NormalizeRows(existingValues, existingRowsCount);
            var existingRows = _sheetParser.ParseCsv(_csvWriter.ToCsv(normalizedValues)).Cast<SkinDataModel>().ToList();

            var skins = _sourcesParser.ParseSkins(skinsJson);
            var prices = _sourcesParser.ParsePrices(pricesJson);

            var rowsBuilder = new SkinsSheetRowsBuilder(_config.ExcludedCategories, _config.GetRarityMap(), _config.SouvenirCrateMarker);
            var buildResult = rowsBuilder.Build(skins, prices);
            var mergeResult = _merger.Merge(existingRows, buildResult.Rows, buildResult.SkippedIds);

            EnsureNoRowsLost(existingRowsCount, existingRows.Count, mergeResult.Rows.Count);

            var values = _csvWriter.BuildValues(mergeResult.Rows);
            var csv = _csvWriter.ToCsv(values);

            return new SkinsSheetExportResult(buildResult, mergeResult, values, _csvWriter.BooleanColumns, csv);
        }

        private Task<string> DownloadSkins(Action<string, float> progressChanged)
        {
            return DownloadTools.DownloadText(_config.SkinsSourceUrl, _config.RequestTimeoutSeconds, progress => progressChanged("Downloading skins", progress));
        }

        private Task<string> DownloadPrices(Action<string, float> progressChanged)
        {
            return DownloadTools.DownloadText(_config.PricesSourceUrl, _config.RequestTimeoutSeconds, progress => progressChanged("Downloading prices", progress));
        }

        private List<List<string>> ReadCsvValues(string csv)
        {
            var values = new List<List<string>>();

            using var parser = new CsvParser(new StringReader(csv), CultureInfo.InvariantCulture);

            while (parser.Read() == true)
            {
                values.Add(parser.Record.ToList());
            }

            return values;
        }

        private int GetLastFilledRowIndex(IReadOnlyList<List<string>> values)
        {
            for (var index = values.Count - 1; index >= HeaderRowsCount; index--)
            {
                if (IsIdEmpty(values[index]) == false)
                {
                    return index;
                }
            }

            return 0;
        }

        private void EnsureNoEmptyIds(IReadOnlyList<List<string>> values, int lastFilledRowIndex)
        {
            var emptyRows = new List<int>();

            for (var index = HeaderRowsCount; index < lastFilledRowIndex; index++)
            {
                if (IsIdEmpty(values[index]) == true)
                {
                    emptyRows.Add(index + 1);
                }
            }

            if (emptyRows.Count > 0)
            {
                throw new InvalidOperationException($"Current sheet has rows with an empty Id between filled rows (sheet rows {string.Join(", ", emptyRows)}), export refused. Fill or delete them first.");
            }
        }

        private bool IsIdEmpty(List<string> row)
        {
            return row.Count <= IdColumn || string.IsNullOrWhiteSpace(row[IdColumn]) == true;
        }

        private List<List<string>> NormalizeRows(IReadOnlyList<List<string>> values, int lastFilledRowIndex)
        {
            var normalizedValues = new List<List<string>>(lastFilledRowIndex + 1);

            for (var index = 0; index <= lastFilledRowIndex && index < values.Count; index++)
            {
                var row = new List<string>(values[index]);

                while (row.Count < ColumnsCount)
                {
                    row.Add(string.Empty);
                }

                normalizedValues.Add(row);
            }

            return normalizedValues;
        }

        private void EnsureNoRowsLost(int existingRowsCount, int parsedRowsCount, int mergedRowsCount)
        {
            if (existingRowsCount == 0)
            {
                throw new InvalidOperationException("Current skins sheet has 0 rows, export refused to avoid losing kept rows");
            }

            if (parsedRowsCount < existingRowsCount)
            {
                throw new InvalidOperationException($"Only {parsedRowsCount} of {existingRowsCount} current sheet rows were parsed, export refused");
            }

            if (mergedRowsCount < existingRowsCount)
            {
                throw new InvalidOperationException($"Merged table has {mergedRowsCount} rows, fewer than {existingRowsCount} in the current sheet, export refused");
            }
        }
    }
}

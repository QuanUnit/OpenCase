using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Features.RemoteConfigs.Editor.Models;
using Shared.Tools.Formats;

namespace Features.RemoteConfigs.Editor.GoogleSheets
{
    public class GoogleSheetsRequestBuilder
    {
        private const string JwtBearerGrantType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        private const string SheetPropertiesFields = "sheets.properties(sheetId,title,gridProperties(rowCount))";
        private const string ValuesKey = "values";
        private const int LettersCount = 26;

        public string BuildAccessTokenForm(string jwt)
        {
            return $"grant_type={Uri.EscapeDataString(JwtBearerGrantType)}&assertion={Uri.EscapeDataString(jwt)}";
        }

        public string BuildSheetPropertiesUrl(string apiUrl, string spreadsheetId)
        {
            return $"{apiUrl}/{spreadsheetId}?fields={Uri.EscapeDataString(SheetPropertiesFields)}";
        }

        public string BuildReadValuesUrl(string apiUrl, string spreadsheetId, string range)
        {
            return $"{apiUrl}/{spreadsheetId}/values/{Uri.EscapeDataString(range)}?valueRenderOption=FORMATTED_VALUE";
        }

        public string BuildUpdateValuesUrl(string apiUrl, string spreadsheetId, string range)
        {
            return $"{apiUrl}/{spreadsheetId}/values/{Uri.EscapeDataString(range)}?valueInputOption=RAW";
        }

        public string BuildClearValuesUrl(string apiUrl, string spreadsheetId, string range)
        {
            return $"{apiUrl}/{spreadsheetId}/values/{Uri.EscapeDataString(range)}:clear";
        }

        public string BuildRange(string sheetTitle, int firstRow, int lastRow, int columnsCount)
        {
            return $"{QuoteSheetTitle(sheetTitle)}!A{firstRow}:{GetColumnLetters(columnsCount)}{lastRow}";
        }

        public string BuildColumnsRange(string sheetTitle, int columnsCount)
        {
            return $"{QuoteSheetTitle(sheetTitle)}!A:{GetColumnLetters(columnsCount)}";
        }

        public string GetColumnLetters(int columnNumber)
        {
            var letters = string.Empty;
            var remaining = columnNumber;

            while (remaining > 0)
            {
                var letterIndex = (remaining - 1) % LettersCount;
                letters = (char)('A' + letterIndex) + letters;
                remaining = (remaining - 1) / LettersCount;
            }

            return letters;
        }

        public string BuildValuesJson(string range, IReadOnlyList<List<string>> values, ICollection<int> booleanColumns)
        {
            var builder = new StringBuilder();

            builder.Append("{\"range\":").AppendJsonString(range);
            builder.Append(",\"majorDimension\":\"ROWS\",\"values\":[");

            for (var rowIndex = 0; rowIndex < values.Count; rowIndex++)
            {
                if (rowIndex > 0)
                {
                    builder.Append(',');
                }

                AppendRow(builder, values[rowIndex], rowIndex, booleanColumns);
            }

            builder.Append("]}");
            return builder.ToString();
        }

        public List<List<string>> ParseValues(string responseJson)
        {
            var response = (Dictionary<string, object>)new JsonReader(responseJson).Read();
            var values = new List<List<string>>();

            if (response.TryGetValue(ValuesKey, out var rows) == false)
            {
                return values;
            }

            foreach (List<object> row in (List<object>)rows)
            {
                var cells = new List<string>(row.Count);

                foreach (var cell in row)
                {
                    cells.Add(Convert.ToString(cell, CultureInfo.InvariantCulture));
                }

                values.Add(cells);
            }

            return values;
        }

        public GoogleSheetProperties FindSheet(GoogleSpreadsheetResponse response, string pageId)
        {
            foreach (var sheet in response.sheets)
            {
                if (sheet.properties.sheetId.ToString(CultureInfo.InvariantCulture) == pageId)
                {
                    return sheet.properties;
                }
            }

            throw new InvalidOperationException($"Sheet with gid {pageId} not found in the spreadsheet");
        }

        private void AppendRow(StringBuilder builder, List<string> row, int rowIndex, ICollection<int> booleanColumns)
        {
            builder.Append('[');

            for (var columnIndex = 0; columnIndex < row.Count; columnIndex++)
            {
                if (columnIndex > 0)
                {
                    builder.Append(',');
                }

                if (rowIndex > 0 && booleanColumns.Contains(columnIndex) == true)
                {
                    builder.Append(ToJsonBoolean(row[columnIndex]));
                }
                else
                {
                    builder.AppendJsonString(row[columnIndex]);
                }
            }

            builder.Append(']');
        }

        private string ToJsonBoolean(string value)
        {
            switch (value)
            {
                case SheetBooleanValues.True:
                    return "true";
                case SheetBooleanValues.False:
                    return "false";
                default:
                    throw new FormatException($"Boolean cell expected {SheetBooleanValues.True} or {SheetBooleanValues.False}, got '{value}'");
            }
        }

        private string QuoteSheetTitle(string sheetTitle)
        {
            return $"'{sheetTitle.Replace("'", "''")}'";
        }
    }
}

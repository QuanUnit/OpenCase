namespace Features.RemoteConfigs.Editor.GoogleSheets
{
    public class GoogleSheetSession
    {
        public readonly string SpreadsheetId;
        public readonly string AccessToken;
        public readonly string SheetTitle;
        public readonly int RowCount;

        public GoogleSheetSession(string spreadsheetId, string accessToken, string sheetTitle, int rowCount)
        {
            SpreadsheetId = spreadsheetId;
            AccessToken = accessToken;
            SheetTitle = sheetTitle;
            RowCount = rowCount;
        }
    }
}

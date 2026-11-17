using System;

namespace Features.RemoteConfigs.Editor.GoogleSheets
{
    [Serializable]
    public class GoogleAccessTokenResponse
    {
        public string access_token;
        public string error;
        public string error_description;
    }

    [Serializable]
    public class GoogleApiErrorResponse
    {
        public GoogleApiError error;
    }

    [Serializable]
    public class GoogleApiError
    {
        public int code;
        public string message;
        public string status;
    }

    [Serializable]
    public class GoogleSpreadsheetResponse
    {
        public GoogleSheet[] sheets;
    }

    [Serializable]
    public class GoogleSheet
    {
        public GoogleSheetProperties properties;
    }

    [Serializable]
    public class GoogleSheetProperties
    {
        public int sheetId;
        public string title;
        public GoogleGridProperties gridProperties;
    }

    [Serializable]
    public class GoogleGridProperties
    {
        public int rowCount;
    }

    [Serializable]
    public class GoogleUpdateValuesResponse
    {
        public int updatedRows;
    }
}

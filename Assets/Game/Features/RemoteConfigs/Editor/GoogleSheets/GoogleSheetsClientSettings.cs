namespace Features.RemoteConfigs.Editor.GoogleSheets
{
    public class GoogleSheetsClientSettings
    {
        public readonly string ApiUrl;
        public readonly string Scope;
        public readonly int AccessTokenLifetimeSeconds;
        public readonly int IssuedAtMarginSeconds;
        public readonly int MaxResponseLengthInErrors;

        public GoogleSheetsClientSettings(string apiUrl, string scope, int accessTokenLifetimeSeconds, int issuedAtMarginSeconds, int maxResponseLengthInErrors)
        {
            ApiUrl = apiUrl;
            Scope = scope;
            AccessTokenLifetimeSeconds = accessTokenLifetimeSeconds;
            IssuedAtMarginSeconds = issuedAtMarginSeconds;
            MaxResponseLengthInErrors = maxResponseLengthInErrors;
        }
    }
}

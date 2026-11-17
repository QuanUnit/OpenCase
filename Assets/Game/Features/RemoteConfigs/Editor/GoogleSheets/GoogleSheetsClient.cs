using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Features.RemoteConfigs.Editor.GoogleSheets
{
    public class GoogleSheetsClient
    {
        private const string JsonContentType = "application/json";
        private const string FormContentType = "application/x-www-form-urlencoded";
        private const string EmptyJsonBody = "{}";
        private const int FirstRow = 1;
        private const int MaxAccessTokenLifetimeSeconds = 3600;

        private readonly IHttpSender _sender;
        private readonly GoogleSheetsClientSettings _settings;
        private readonly ServiceAccountKeyPathValidator _keyPathValidator;
        private readonly GoogleServiceAccountJwtBuilder _jwtBuilder;
        private readonly GoogleSheetsRequestBuilder _requestBuilder;

        public GoogleSheetsClient(IHttpSender sender, GoogleSheetsClientSettings settings, ServiceAccountKeyPathValidator keyPathValidator)
        {
            _sender = sender;
            _settings = settings;
            _keyPathValidator = keyPathValidator;
            _jwtBuilder = new GoogleServiceAccountJwtBuilder();
            _requestBuilder = new GoogleSheetsRequestBuilder();
        }

        public async Task<GoogleSheetSession> Connect(string keyPath, string spreadsheetId, string pageId)
        {
            _keyPathValidator.EnsureOutsideProject(keyPath);

            var key = ReadKey(keyPath);
            var accessToken = await GetAccessToken(key);
            var sheet = await GetSheet(spreadsheetId, pageId, accessToken);

            return new GoogleSheetSession(spreadsheetId, accessToken, sheet.title, sheet.gridProperties.rowCount);
        }

        public async Task<List<List<string>>> ReadValues(GoogleSheetSession session, int columnsCount)
        {
            var range = _requestBuilder.BuildColumnsRange(session.SheetTitle, columnsCount);
            var url = _requestBuilder.BuildReadValuesUrl(_settings.ApiUrl, session.SpreadsheetId, range);

            var response = await _sender.Send(UnityWebRequest.kHttpVerbGET, url, null, null, session.AccessToken);
            EnsureSuccess(response, "Reading current values");

            return _requestBuilder.ParseValues(response.Text);
        }

        public async Task<int> WriteValues(GoogleSheetSession session, IReadOnlyList<List<string>> values, ICollection<int> booleanColumns)
        {
            var columnsCount = values[0].Count;
            var range = _requestBuilder.BuildRange(session.SheetTitle, FirstRow, values.Count, columnsCount);
            var updateUrl = _requestBuilder.BuildUpdateValuesUrl(_settings.ApiUrl, session.SpreadsheetId, range);
            var body = _requestBuilder.BuildValuesJson(range, values, booleanColumns);

            var updateResponse = await _sender.Send(UnityWebRequest.kHttpVerbPUT, updateUrl, body, JsonContentType, session.AccessToken);
            EnsureSuccess(updateResponse, "Writing values");

            var updatedRows = JsonUtility.FromJson<GoogleUpdateValuesResponse>(updateResponse.Text).updatedRows;

            if (values.Count < session.RowCount)
            {
                await ClearLeftoverRows(session, values.Count + 1, columnsCount);
            }

            return updatedRows;
        }

        private async Task ClearLeftoverRows(GoogleSheetSession session, int firstRow, int columnsCount)
        {
            var range = _requestBuilder.BuildRange(session.SheetTitle, firstRow, session.RowCount, columnsCount);
            var url = _requestBuilder.BuildClearValuesUrl(_settings.ApiUrl, session.SpreadsheetId, range);

            var response = await _sender.Send(UnityWebRequest.kHttpVerbPOST, url, EmptyJsonBody, JsonContentType, session.AccessToken);

            if (response.IsSuccess == false)
            {
                throw new InvalidOperationException($"Values written, leftover clear failed: {GetApiErrorMessage(response)}");
            }
        }

        private GoogleServiceAccountKey ReadKey(string keyPath)
        {
            GoogleServiceAccountKey key;

            try
            {
                key = JsonUtility.FromJson<GoogleServiceAccountKey>(File.ReadAllText(keyPath));
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException("Service account key file is not valid JSON");
            }

            if (string.IsNullOrEmpty(key.client_email) == true || string.IsNullOrEmpty(key.private_key) == true || string.IsNullOrEmpty(key.token_uri) == true)
            {
                throw new InvalidOperationException("Service account key file must contain client_email, private_key and token_uri");
            }

            return key;
        }

        private async Task<string> GetAccessToken(GoogleServiceAccountKey key)
        {
            var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _settings.IssuedAtMarginSeconds;
            var lifetime = Math.Min(_settings.AccessTokenLifetimeSeconds, MaxAccessTokenLifetimeSeconds);
            var jwt = _jwtBuilder.Build(key, _settings.Scope, issuedAt, lifetime);
            var form = _requestBuilder.BuildAccessTokenForm(jwt);

            var response = await _sender.Send(UnityWebRequest.kHttpVerbPOST, key.token_uri, form, FormContentType, null);
            var tokenResponse = ParseOrDefault<GoogleAccessTokenResponse>(response.Text);

            if (response.IsSuccess == true && tokenResponse != null && string.IsNullOrEmpty(tokenResponse.access_token) == false)
            {
                return tokenResponse.access_token;
            }

            var reason = tokenResponse != null && string.IsNullOrEmpty(tokenResponse.error) == false
                ? $"{tokenResponse.error}: {Truncate(tokenResponse.error_description ?? string.Empty)}"
                : Truncate(response.Text);

            throw new InvalidOperationException($"Google access token request failed ({response.Code} {response.Error}): {reason}");
        }

        private async Task<GoogleSheetProperties> GetSheet(string spreadsheetId, string pageId, string accessToken)
        {
            var url = _requestBuilder.BuildSheetPropertiesUrl(_settings.ApiUrl, spreadsheetId);

            var response = await _sender.Send(UnityWebRequest.kHttpVerbGET, url, null, null, accessToken);
            EnsureSuccess(response, "Reading spreadsheet sheets");

            var spreadsheet = JsonUtility.FromJson<GoogleSpreadsheetResponse>(response.Text);

            return _requestBuilder.FindSheet(spreadsheet, pageId);
        }

        private void EnsureSuccess(HttpResponse response, string operationName)
        {
            if (response.IsSuccess == true)
            {
                return;
            }

            throw new InvalidOperationException($"{operationName} failed: {GetApiErrorMessage(response)}");
        }

        private string GetApiErrorMessage(HttpResponse response)
        {
            var errorResponse = ParseOrDefault<GoogleApiErrorResponse>(response.Text);

            if (errorResponse != null && errorResponse.error != null && string.IsNullOrEmpty(errorResponse.error.message) == false)
            {
                return $"({response.Code} {response.Error}) {errorResponse.error.status}: {Truncate(errorResponse.error.message)}";
            }

            return $"({response.Code} {response.Error}) {Truncate(response.Text)}";
        }

        private T ParseOrDefault<T>(string text) where T : class
        {
            if (string.IsNullOrEmpty(text) == true || text.TrimStart().StartsWith("{", StringComparison.Ordinal) == false)
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<T>(text);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private string Truncate(string text)
        {
            if (text.Length <= _settings.MaxResponseLengthInErrors)
            {
                return text;
            }

            return $"{text.Substring(0, _settings.MaxResponseLengthInErrors)}... ({text.Length} characters)";
        }
    }
}

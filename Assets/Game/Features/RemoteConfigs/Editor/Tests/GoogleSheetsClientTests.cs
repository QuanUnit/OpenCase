using System;
using System.Collections.Generic;
using System.IO;
using Features.RemoteConfigs.Editor.GoogleSheets;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class GoogleSheetsClientTests
    {
        private const string ApiUrl = "https://api";
        private const string TokenResponse = "{\"access_token\":\"token-1\",\"expires_in\":3599,\"token_type\":\"Bearer\"}";
        private const string SheetsResponse = "{\"sheets\":[{\"properties\":{\"sheetId\":0,\"title\":\"Skins\",\"gridProperties\":{\"rowCount\":10}}}]}";

        private string _temporaryFolder;
        private string _keyPath;
        private FakeHttpSender _sender;
        private GoogleSheetsClient _client;

        [SetUp]
        public void SetUp()
        {
            _temporaryFolder = Path.Combine(Path.GetTempPath(), $"OpenCaseClientTests{Guid.NewGuid():N}");
            Directory.CreateDirectory(_temporaryFolder);

            _keyPath = Path.Combine(_temporaryFolder, "key.json");
            File.WriteAllText(_keyPath, BuildKeyJson());

            _sender = new FakeHttpSender();

            var settings = new GoogleSheetsClientSettings(ApiUrl, "https://www.googleapis.com/auth/spreadsheets", 3600, 30, 300);
            _client = new GoogleSheetsClient(_sender, settings, new ServiceAccountKeyPathValidator(Path.Combine(_temporaryFolder, "Project")));
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_temporaryFolder, true);
        }

        [Test]
        public void ConnectAndWrite_ShorterTable_WritesThenClearsExistingLeftoverRows()
        {
            _sender.Enqueue(true, 200, TokenResponse);
            _sender.Enqueue(true, 200, SheetsResponse);
            _sender.Enqueue(true, 200, "{\"updatedRows\":3}");
            _sender.Enqueue(true, 200, "{}");

            var session = _client.Connect(_keyPath, "sheet", "0").GetAwaiter().GetResult();
            var updatedRows = _client.WriteValues(session, CreateValues(), new HashSet<int> { 1 }).GetAwaiter().GetResult();

            Assert.That(updatedRows, Is.EqualTo(3));
            Assert.That(_sender.Requests, Is.EqualTo(new[]
            {
                $"POST {TestServiceAccountKey.TokenUri} ",
                $"GET {ApiUrl}/sheet?fields=sheets.properties%28sheetId%2Ctitle%2CgridProperties%28rowCount%29%29 token-1",
                $"PUT {ApiUrl}/sheet/values/%27Skins%27%21A1%3AB3?valueInputOption=RAW token-1",
                $"POST {ApiUrl}/sheet/values/%27Skins%27%21A4%3AB10:clear token-1",
            }));
            Assert.That(_sender.Bodies[0], Does.StartWith("grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Ajwt-bearer&assertion="));
            Assert.That(_sender.Bodies[2], Does.Contain("[\"A\",true]"));
        }

        [Test]
        public void WriteValues_TableFillsGrid_DoesNotClear()
        {
            var session = new GoogleSheetSession("sheet", "token-1", "Skins", 3);
            _sender.Enqueue(true, 200, "{\"updatedRows\":3}");

            _client.WriteValues(session, CreateValues(), new HashSet<int> { 1 }).GetAwaiter().GetResult();

            Assert.That(_sender.Requests.Count, Is.EqualTo(1));
        }

        [Test]
        public void WriteValues_PutFails_DoesNotClearAndReportsGoogleMessage()
        {
            var session = new GoogleSheetSession("sheet", "token-1", "Skins", 10);
            _sender.Enqueue(false, 403, "{\"error\":{\"code\":403,\"message\":\"The caller does not have permission\",\"status\":\"PERMISSION_DENIED\"}}");

            var exception = Assert.Throws<InvalidOperationException>(() => _client.WriteValues(session, CreateValues(), new HashSet<int> { 1 }).GetAwaiter().GetResult());

            Assert.That(exception.Message, Does.Contain("PERMISSION_DENIED: The caller does not have permission"));
            Assert.That(_sender.Requests.Count, Is.EqualTo(1));
        }

        [Test]
        public void WriteValues_ClearFails_ReportsValuesWritten()
        {
            var session = new GoogleSheetSession("sheet", "token-1", "Skins", 10);
            _sender.Enqueue(true, 200, "{\"updatedRows\":3}");
            _sender.Enqueue(false, 400, "{\"error\":{\"code\":400,\"message\":\"exceeds grid limits\",\"status\":\"INVALID_ARGUMENT\"}}");

            var exception = Assert.Throws<InvalidOperationException>(() => _client.WriteValues(session, CreateValues(), new HashSet<int> { 1 }).GetAwaiter().GetResult());

            Assert.That(exception.Message, Does.StartWith("Values written, leftover clear failed:"));
        }

        [Test]
        public void Connect_TokenConnectionError_IncludesRequestError()
        {
            _sender.Enqueue(false, 0, string.Empty);

            var exception = Assert.Throws<InvalidOperationException>(() => _client.Connect(_keyPath, "sheet", "0").GetAwaiter().GetResult());

            Assert.That(exception.Message, Does.Contain("HTTP/1.1 error"));
            Assert.That(exception.Message, Does.Not.Contain("BEGIN PRIVATE KEY"));
        }

        [Test]
        public void Connect_KeyInsideProject_Throws()
        {
            var projectKeyPath = Path.Combine(_temporaryFolder, "Project", "Assets", "key.json");

            var exception = Assert.Throws<InvalidOperationException>(() => _client.Connect(projectKeyPath, "sheet", "0").GetAwaiter().GetResult());

            Assert.That(exception.Message, Does.Contain("outside the project folder"));
            Assert.That(_sender.Requests, Is.Empty);
        }

        [Test]
        public void ReadValues_Session_ReadsColumnsRange()
        {
            var session = new GoogleSheetSession("sheet", "token-1", "Skins", 10);
            _sender.Enqueue(true, 200, "{\"values\":[[\"Id\"],[\"AK-47_Inheritance\"]]}");

            var values = _client.ReadValues(session, 6).GetAwaiter().GetResult();

            Assert.That(values.Count, Is.EqualTo(2));
            Assert.That(_sender.Requests[0], Is.EqualTo($"GET {ApiUrl}/sheet/values/%27Skins%27%21A%3AF?valueRenderOption=FORMATTED_VALUE token-1"));
        }

        private List<List<string>> CreateValues()
        {
            return new List<List<string>>
            {
                new List<string> { "Id", "Flag" },
                new List<string> { "A", "TRUE" },
                new List<string> { "B", "FALSE" },
            };
        }

        private string BuildKeyJson()
        {
            var key = new GoogleServiceAccountKey
            {
                client_email = TestServiceAccountKey.ClientEmail,
                private_key = TestServiceAccountKey.Pem,
                token_uri = TestServiceAccountKey.TokenUri,
            };

            return UnityEngine.JsonUtility.ToJson(key);
        }
    }
}

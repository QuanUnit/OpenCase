using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Features.RemoteConfigs.Editor.Cases;
using Features.RemoteConfigs.Editor.GoogleSheets;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class CasesSheetUploaderTests
    {
        private const string ApiUrl = "https://api";
        private const string PageId = "686572383";
        private const string TokenResponse = "{\"access_token\":\"token-1\",\"expires_in\":3599,\"token_type\":\"Bearer\"}";
        private const string SheetsResponse = "{\"sheets\":[{\"properties\":{\"sheetId\":686572383,\"title\":\"Cases\",\"gridProperties\":{\"rowCount\":10}}}]}";
        private const string ForeignValuesResponse = "{\"values\":[[\"Id\",\"Name\",\"Price\",\"SkinId\",\"Weight\"],[\"Case_Old\",\"Old\",\"1\",\"Skin\",\"1\"]]}";
        private const string OwnValuesResponse = "{\"values\":[[\"Id\",\"Name\",\"Price\",\"SkinId\",\"Weight\"],[\"Case_A\",\"A\",\"1\",\"Skin\",\"1\"]]}";

        private string _temporaryFolder;
        private string _keyPath;
        private FakeHttpSender _sender;
        private CasesSheetUploader _uploader;
        private List<CasesSheetCase> _cases;

        [SetUp]
        public void SetUp()
        {
            _temporaryFolder = Path.Combine(Path.GetTempPath(), $"OpenCaseUploaderTests{Guid.NewGuid():N}");
            Directory.CreateDirectory(_temporaryFolder);

            _keyPath = Path.Combine(_temporaryFolder, "key.json");
            File.WriteAllText(_keyPath, BuildKeyJson());

            _sender = new FakeHttpSender();

            var settings = new GoogleSheetsClientSettings(ApiUrl, "https://www.googleapis.com/auth/spreadsheets", 3600, 30, 300);
            var client = new GoogleSheetsClient(_sender, settings, new ServiceAccountKeyPathValidator(Path.Combine(_temporaryFolder, "Project")));
            _uploader = new CasesSheetUploader(client);

            var skins = new List<KeyValuePair<string, float>> { new KeyValuePair<string, float>("AK-47_Redline", 1f) };
            _cases = new List<CasesSheetCase> { new CasesSheetCase("Case_A", "A", 1f, skins) };
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_temporaryFolder, true);
        }

        [Test]
        public void Upload_ForeignCasesAndCancel_DoesNotWrite()
        {
            _sender.Enqueue(true, 200, TokenResponse);
            _sender.Enqueue(true, 200, SheetsResponse);
            _sender.Enqueue(true, 200, ForeignValuesResponse);

            IReadOnlyList<string> askedIds = null;

            var writtenRows = _uploader.Upload(_keyPath, "sheet", PageId, _cases, ids =>
            {
                askedIds = ids;
                return false;
            }).GetAwaiter().GetResult();

            Assert.That(writtenRows, Is.EqualTo(0));
            Assert.That(askedIds, Is.EqualTo(new[] { "Case_Old" }));
            Assert.That(_sender.Requests.Count, Is.EqualTo(3));
            Assert.That(_sender.Requests.Any(request => request.StartsWith("PUT") == true || request.Contains(":clear") == true), Is.False);
        }

        [Test]
        public void Upload_ForeignCasesAndConfirm_WritesBuiltRows()
        {
            _sender.Enqueue(true, 200, TokenResponse);
            _sender.Enqueue(true, 200, SheetsResponse);
            _sender.Enqueue(true, 200, ForeignValuesResponse);
            _sender.Enqueue(true, 200, "{\"updatedRows\":2}");
            _sender.Enqueue(true, 200, "{}");

            var writtenRows = _uploader.Upload(_keyPath, "sheet", PageId, _cases, ids => true).GetAwaiter().GetResult();

            Assert.That(writtenRows, Is.EqualTo(2));
            Assert.That(_sender.Requests[3], Does.StartWith($"PUT {ApiUrl}/sheet/values/%27Cases%27%21A1%3AE2"));
            Assert.That(_sender.Bodies[3], Does.Contain("[\"Case_A\",\"A\",\"1,00\",\"AK-47_Redline\",\"1\"]"));
        }

        [Test]
        public void Upload_OnlyOwnCases_DoesNotAsk()
        {
            _sender.Enqueue(true, 200, TokenResponse);
            _sender.Enqueue(true, 200, SheetsResponse);
            _sender.Enqueue(true, 200, OwnValuesResponse);
            _sender.Enqueue(true, 200, "{\"updatedRows\":2}");
            _sender.Enqueue(true, 200, "{}");

            var isAsked = false;

            _uploader.Upload(_keyPath, "sheet", PageId, _cases, ids =>
            {
                isAsked = true;
                return false;
            }).GetAwaiter().GetResult();

            Assert.That(isAsked, Is.False);
            Assert.That(_sender.Requests.Count, Is.EqualTo(5));
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

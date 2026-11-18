using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Features.Cases.Configs;
using Features.RemoteConfigs.Editor.GoogleSheets;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CasesSheetPusher
    {
        private readonly CaseEditorConfig _config;
        private readonly ServiceAccountKeyPathValidator _keyPathValidator = new ServiceAccountKeyPathValidator(Directory.GetParent(Application.dataPath).FullName);

        public CasesSheetPusher(CaseEditorConfig config)
        {
            _config = config;
        }

        public string GetKeyPath()
        {
            return EditorPrefs.GetString(GoogleSheetsPrefs.ServiceAccountKeyPathKey, string.Empty);
        }

        public bool IsKeyUsable()
        {
            var keyPath = GetKeyPath();

            return string.IsNullOrEmpty(keyPath) == false && File.Exists(keyPath) == true && _keyPathValidator.IsInsideProject(keyPath) == false;
        }

        public List<CasesSheetCase> BuildCases(IReadOnlyList<CaseConfig> cases)
        {
            return cases.Select(caseConfig => new CasesSheetCase(
                    caseConfig.CaseStringId,
                    caseConfig.Name,
                    caseConfig.Price,
                    caseConfig.SkinsMapEntries == null
                        ? new List<KeyValuePair<string, float>>()
                        : caseConfig.SkinsMapEntries.Entries
                            .Where(entry => entry.Key != null)
                            .Select(entry => new KeyValuePair<string, float>(entry.Key.SkinStringId, entry.Value))
                            .ToList()))
                .ToList();
        }

        public Task<int> Push(IReadOnlyList<CaseConfig> cases, Func<IReadOnlyList<string>, bool> confirmUnknownIds)
        {
            var sender = new UnityWebRequestSender(_config.SheetsApiConfig.RequestTimeoutSeconds);
            var client = new GoogleSheetsClient(sender, _config.SheetsApiConfig.GetGoogleSheetsClientSettings(), _keyPathValidator);
            var uploader = new CasesSheetUploader(client);

            return uploader.Upload(GetKeyPath(), _config.RemoteConfigsData.SheetId, _config.GetCasesPage().Id, BuildCases(cases), confirmUnknownIds);
        }
    }
}

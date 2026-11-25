using System;
using System.Collections.Generic;
using System.Linq;
using Features.RemoteConfigs.Editor.GoogleSheets;
using Features.Skins.Data;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Exporters.Skins
{
    [CreateAssetMenu(menuName = "Configs/Editor/" + nameof(SkinsSheetExportConfig), fileName = nameof(SkinsSheetExportConfig))]
    public class SkinsSheetExportConfig : ScriptableObject
    {
        public string SkinsSourceUrl => _skinsSourceUrl;
        public string PricesSourceUrl => _pricesSourceUrl;
        public string SheetId => _remoteConfigsData.SheetId;
        public string PageId => _remoteConfigsData.Pages.First(page => page.Name == _pageName).Id;
        public int RequestTimeoutSeconds => _requestTimeoutSeconds;
        public IReadOnlyCollection<string> ExcludedCategories => _excludedCategories;
        public string SouvenirCrateMarker => _souvenirCrateMarker;

        [Header("Sources")]
        [SerializeField] private string _skinsSourceUrl;
        [SerializeField] private string _pricesSourceUrl;

        [Header("Target sheet")]
        [SerializeField] private EditorRemoteConfigsData _remoteConfigsData;
        [SerializeField] private string _pageName;

        [Header("Google Sheets API")]
        [SerializeField] private string _sheetsApiUrl;
        [SerializeField] private string _accessScope;
        [SerializeField, Range(1, 3600)] private int _accessTokenLifetimeSeconds = 3600;
        [SerializeField, Min(0)] private int _issuedAtMarginSeconds = 30;
        [SerializeField, Min(0)] private int _maxResponseLengthInErrors = 300;
        [SerializeField, Min(1)] private int _requestTimeoutSeconds = 60;

        [Header("Rows")]
        [SerializeField] private List<string> _excludedCategories = new List<string>();
        [SerializeField] private string _souvenirCrateMarker = "Souvenir";

        [SerializeField] private List<RarityMapping> _rarityMappings = new List<RarityMapping>
        {
            new RarityMapping { SourceName = "Consumer Grade", Rarity = Rarity.ConsumerGrade },
            new RarityMapping { SourceName = "Industrial Grade", Rarity = Rarity.IndustrialGrade },
            new RarityMapping { SourceName = "Mil-Spec Grade", Rarity = Rarity.MilSpec },
            new RarityMapping { SourceName = "Restricted", Rarity = Rarity.Restricted },
            new RarityMapping { SourceName = "Classified", Rarity = Rarity.Classified },
            new RarityMapping { SourceName = "Covert", Rarity = Rarity.Covert },
            new RarityMapping { SourceName = "Contraband", Rarity = Rarity.RareSpecial },
            new RarityMapping { SourceName = "Extraordinary", Rarity = Rarity.RareSpecial },
        };

        [SerializeField] private List<RarityMapping> _categoryRarityMappings = new List<RarityMapping>
        {
            new RarityMapping { SourceName = "Knives", Rarity = Rarity.RareSpecial },
            new RarityMapping { SourceName = "Gloves", Rarity = Rarity.RareSpecial },
        };

        public GoogleSheetsClientSettings GetGoogleSheetsClientSettings()
        {
            return new GoogleSheetsClientSettings(_sheetsApiUrl, _accessScope, _accessTokenLifetimeSeconds, _issuedAtMarginSeconds, _maxResponseLengthInErrors);
        }

        public Dictionary<string, Rarity> GetRarityMap()
        {
            return ToDictionary(_rarityMappings);
        }

        public Dictionary<string, Rarity> GetCategoryRarityMap()
        {
            return ToDictionary(_categoryRarityMappings);
        }

        private Dictionary<string, Rarity> ToDictionary(List<RarityMapping> mappings)
        {
            var rarityMap = new Dictionary<string, Rarity>();

            foreach (var rarityMapping in mappings)
            {
                rarityMap[rarityMapping.SourceName] = rarityMapping.Rarity;
            }

            return rarityMap;
        }

        [Serializable]
        public struct RarityMapping
        {
            public string SourceName;
            public Rarity Rarity;
        }
    }
}

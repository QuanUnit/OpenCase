using System;
using System.Collections.Generic;
using System.Linq;
using Features.Cases.Configs;
using Features.Clicker.Configs;
using Features.RemoteConfigs.Editor.Exporters.Skins;
using Features.Skins.Configs;
using Features.Skins.Data;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Cases
{
    [CreateAssetMenu(menuName = "Configs/Editor/" + nameof(CaseEditorConfig), fileName = nameof(CaseEditorConfig))]
    public class CaseEditorConfig : ScriptableObject
    {
        public AllCasesConfig AllCasesConfig => _allCasesConfig;
        public SkinsGlobalSettingsConfig SkinsGlobalSettingsConfig => _skinsGlobalSettingsConfig;
        public SkinsUIContextDataConfig SkinsUIContextDataConfig => _skinsUIContextDataConfig;
        public ClickerConfig ClickerConfig => _clickerConfig;
        public string CasesFolderPath => AssetDatabase.GetAssetPath(_casesFolder);
        public Sprite DefaultCaseIcon => _defaultCaseIcon;
        public EditorRemoteConfigsData RemoteConfigsData => _remoteConfigsData;
        public string CasesPageName => _casesPageName;
        public SkinsSheetExportConfig SheetsApiConfig => _sheetsApiConfig;
        public int ValueSamplesCount => _valueSamplesCount;
        public float OddsTotalWeight => _oddsTotalWeight;
        public float DefaultSkinWeight => _defaultSkinWeight;
        public string NewCaseIdPrefix => _newCaseIdPrefix;
        public string NewCaseName => _newCaseName;
        public float NewCasePrice => _newCasePrice;
        public string DuplicateIdSuffix => _duplicateIdSuffix;
        public string NewCategoryName => _newCategoryName;
        public float PriceStep => _priceStep;
        public float DefaultTargetReturnToPlayer => _defaultTargetReturnToPlayer;
        public IReadOnlyList<float> PriceMultipliers => _priceMultipliers;
        public Rarity RareRarity => _rareRarity;
        public int TopContributorsCount => _topContributorsCount;
        public float MaxReturnToPlayer => _maxReturnToPlayer;
        public float MinReturnToPlayer => _minReturnToPlayer;
        public int DefaultSimulationOpens => _defaultSimulationOpens;
        public Color ErrorColor => _errorColor;
        public Color WarningColor => _warningColor;
        public Color GoodColor => _goodColor;
        public float DominantShare => _dominantShare;
        public float CommonRarityChance => _commonRarityChance;

        [Header("Project configs")]
        [SerializeField] private AllCasesConfig _allCasesConfig;
        [SerializeField] private SkinsGlobalSettingsConfig _skinsGlobalSettingsConfig;
        [SerializeField] private SkinsUIContextDataConfig _skinsUIContextDataConfig;
        [SerializeField] private ClickerConfig _clickerConfig;

        [Header("Assets")]
        [SerializeField] private DefaultAsset _casesFolder;
        [SerializeField] private Sprite _defaultCaseIcon;

        [Header("Sheet")]
        [SerializeField] private EditorRemoteConfigsData _remoteConfigsData;
        [SerializeField] private string _casesPageName = "Cases";
        [SerializeField] private SkinsSheetExportConfig _sheetsApiConfig;

        [Header("New cases")]
        [SerializeField] private string _newCaseIdPrefix = "Case_New_";
        [SerializeField] private string _newCaseName = "New Case";
        [SerializeField, Min(0f)] private float _newCasePrice = 1f;
        [SerializeField] private string _duplicateIdSuffix = "_Copy";
        [SerializeField] private string _newCategoryName = "New Category";
        [SerializeField, Min(0f)] private float _defaultSkinWeight = 1f;

        [Header("Weights")]
        [SerializeField, Min(0f)] private float _oddsTotalWeight = 100f;

        [SerializeField] private List<RarityOdds> _rarityOdds = new List<RarityOdds>
        {
            new RarityOdds { Rarity = Rarity.ConsumerGrade, Odds = 1998f },
            new RarityOdds { Rarity = Rarity.IndustrialGrade, Odds = 399.6f },
            new RarityOdds { Rarity = Rarity.MilSpec, Odds = 79.92f },
            new RarityOdds { Rarity = Rarity.Restricted, Odds = 15.98f },
            new RarityOdds { Rarity = Rarity.Classified, Odds = 3.2f },
            new RarityOdds { Rarity = Rarity.Covert, Odds = 0.64f },
            new RarityOdds { Rarity = Rarity.RareSpecial, Odds = 0.26f },
        };

        [SerializeField] private List<RarityOdds> _fixedRarityShares = new List<RarityOdds>
        {
            new RarityOdds { Rarity = Rarity.RareSpecial, Odds = 0.0026f },
        };

        [Header("Pricing")]
        [SerializeField, Min(0.0001f)] private float _priceStep = 0.01f;
        [SerializeField, Range(0.01f, 2f)] private float _defaultTargetReturnToPlayer = 0.9f;

        [Header("Stats")]
        [SerializeField, Min(2)] private int _valueSamplesCount = 512;
        [SerializeField] private List<float> _priceMultipliers = new List<float> { 2f, 5f, 10f };
        [SerializeField] private Rarity _rareRarity = Rarity.Covert;
        [SerializeField, Min(1)] private int _topContributorsCount = 5;
        [SerializeField, Min(0f)] private float _maxReturnToPlayer = 1f;
        [SerializeField, Min(0f)] private float _minReturnToPlayer = 0.6f;
        [SerializeField, Min(1)] private int _defaultSimulationOpens = 100000;
        [SerializeField, Range(0f, 1f)] private float _dominantShare = 0.4f;
        [SerializeField, Range(0f, 1f)] private float _commonRarityChance = 0.1f;

        [Header("Colors")]
        [SerializeField] private Color _errorColor = new Color(1f, 0.35f, 0.35f);
        [SerializeField] private Color _warningColor = new Color(1f, 0.8f, 0.25f);
        [SerializeField] private Color _goodColor = new Color(0.45f, 0.85f, 0.45f);

        public RarityOddsWeighting CreateRarityOddsWeighting()
        {
            return new RarityOddsWeighting(ToDictionary(_rarityOdds), ToDictionary(_fixedRarityShares), _oddsTotalWeight);
        }

        public CaseStatsSettings GetStatsSettings()
        {
            return new CaseStatsSettings(_priceMultipliers, _rareRarity, _topContributorsCount, _clickerConfig.RewardPerClick, _dominantShare, _commonRarityChance);
        }

        public EditorRemoteConfigsData.Page GetCasesPage()
        {
            return _remoteConfigsData.Pages.First(page => page.Name == _casesPageName);
        }

        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            AddIfMissing(errors, _allCasesConfig, nameof(_allCasesConfig));
            AddIfMissing(errors, _skinsGlobalSettingsConfig, nameof(_skinsGlobalSettingsConfig));
            AddIfMissing(errors, _skinsUIContextDataConfig, nameof(_skinsUIContextDataConfig));
            AddIfMissing(errors, _clickerConfig, nameof(_clickerConfig));
            AddIfMissing(errors, _casesFolder, nameof(_casesFolder));
            AddIfMissing(errors, _defaultCaseIcon, nameof(_defaultCaseIcon));
            AddIfMissing(errors, _remoteConfigsData, nameof(_remoteConfigsData));
            AddIfMissing(errors, _sheetsApiConfig, nameof(_sheetsApiConfig));

            if (_remoteConfigsData != null && _remoteConfigsData.Pages.Any(page => page.Name == _casesPageName) == false)
            {
                errors.Add($"Page '{_casesPageName}' not found in {_remoteConfigsData.name}");
            }

            AddDuplicates(errors, _rarityOdds, nameof(_rarityOdds));
            AddDuplicates(errors, _fixedRarityShares, nameof(_fixedRarityShares));

            if (_fixedRarityShares.Sum(entry => entry.Odds) >= 1f)
            {
                errors.Add($"{nameof(_fixedRarityShares)} must sum to less than 1");
            }

            return errors;
        }

        private void AddIfMissing(List<string> errors, UnityEngine.Object value, string fieldName)
        {
            if (value == null)
            {
                errors.Add($"{fieldName} is not assigned");
            }
        }

        private void AddDuplicates(List<string> errors, List<RarityOdds> entries, string fieldName)
        {
            foreach (var group in entries.GroupBy(entry => entry.Rarity).Where(group => group.Count() > 1))
            {
                errors.Add($"{fieldName} has {group.Key} more than once");
            }
        }

        private Dictionary<Rarity, float> ToDictionary(List<RarityOdds> entries)
        {
            var result = new Dictionary<Rarity, float>();

            foreach (var entry in entries)
            {
                result[entry.Rarity] = entry.Odds;
            }

            return result;
        }

        [Serializable]
        public struct RarityOdds
        {
            public Rarity Rarity;
            public float Odds;
        }
    }
}

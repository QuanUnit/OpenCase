using System;
using System.Collections.Generic;
using System.Linq;
using Features.RemoteConfigs.Editor.Icons;
using Features.Skins.Configs;
using Shared.Editor.SpriteAtlases;
using Shared.Tools.AssetsExtensions;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class SkinCatalog
    {
        private const char IdSeparator = '_';
        private const char NameSeparator = ' ';

        public IReadOnlyList<CaseSkinInfo> Skins => _skins;
        public IReadOnlyList<string> Weapons => _weapons;

        private readonly SkinsGlobalSettingsConfig _globalSettingsConfig;
        private readonly SkinValueSampler _sampler;
        private readonly List<CaseSkinInfo> _skins = new List<CaseSkinInfo>();
        private readonly List<string> _weapons = new List<string>();
        private readonly Dictionary<SkinConfig, CaseSkinInfo> _skinsByConfig = new Dictionary<SkinConfig, CaseSkinInfo>();
        private readonly Dictionary<SkinConfig, CachedDistribution> _distributions = new Dictionary<SkinConfig, CachedDistribution>();
        private readonly Dictionary<SkinConfig, CachedIcon> _icons = new Dictionary<SkinConfig, CachedIcon>();
        private readonly Dictionary<SkinConfig, CachedIconPresence> _iconPresence = new Dictionary<SkinConfig, CachedIconPresence>();

        private SkinValueModel _valueModel;
        private int _valueModelFingerprint;

        public SkinCatalog(SkinsGlobalSettingsConfig globalSettingsConfig, int samplesCount)
        {
            _globalSettingsConfig = globalSettingsConfig;
            _sampler = new SkinValueSampler(samplesCount);
        }

        public void Reload()
        {
            _skins.Clear();
            _weapons.Clear();
            _skinsByConfig.Clear();
            _icons.Clear();
            _iconPresence.Clear();

            var configs = AssetDatabaseTools.FindAllAssets<SkinConfig>()
                .Where(config => string.IsNullOrEmpty(config.Id.Value) == false)
                .OrderBy(config => config.Id.Value, StringComparer.Ordinal)
                .ToList();

            var idsWithNames = configs.Select(config => new KeyValuePair<string, string>(config.Id.Value, config.Name ?? string.Empty)).ToList();
            var weapons = new SkinIconWeaponResolver().Resolve(idsWithNames);

            for (var index = 0; index < configs.Count; index++)
            {
                var info = new CaseSkinInfo(configs[index], weapons[index].Replace(IdSeparator, NameSeparator));

                _skins.Add(info);
                _skinsByConfig[configs[index]] = info;
            }

            _weapons.AddRange(_skins.Select(skin => skin.Weapon).Distinct().OrderBy(weapon => weapon, StringComparer.Ordinal));
        }

        public CaseSkinInfo GetInfo(SkinConfig config)
        {
            if (_skinsByConfig.TryGetValue(config, out var info) == true)
            {
                return info;
            }

            info = new CaseSkinInfo(config, config.Id.Value);
            _skinsByConfig[config] = info;

            return info;
        }

        public SkinValueDistribution GetDistribution(SkinConfig config)
        {
            if (_valueModel == null)
            {
                RefreshValueModel();
            }

            if (_distributions.TryGetValue(config, out var cached) == true && cached.IsActual(config, _valueModelFingerprint) == true)
            {
                return cached.Distribution;
            }

            var distribution = _sampler.Sample(_valueModel, config.Price, config.AvailableStatTrak, config.AvailableSouvenir);
            _distributions[config] = new CachedDistribution(config, _valueModelFingerprint, distribution);

            return distribution;
        }

        public Sprite GetIcon(SkinConfig config)
        {
            var guid = GetIconGuid(config);

            if (_icons.TryGetValue(config, out var cached) == true && cached.Guid == guid)
            {
                return cached.Sprite;
            }

            Sprite sprite = null;

            if (string.IsNullOrEmpty(guid) == false)
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid));
            }

            _icons[config] = new CachedIcon(guid, sprite);

            return sprite;
        }

        public bool HasIcon(SkinConfig config)
        {
            var guid = GetIconGuid(config);

            if (_iconPresence.TryGetValue(config, out var cached) == true && cached.Guid == guid)
            {
                return cached.HasIcon;
            }

            var hasIcon = false;

            if (string.IsNullOrEmpty(guid) == false)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                hasIcon = string.IsNullOrEmpty(path) == false && AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(Texture2D);
            }

            _iconPresence[config] = new CachedIconPresence(guid, hasIcon);

            return hasIcon;
        }

        public void InvalidateIcons()
        {
            _icons.Clear();
            _iconPresence.Clear();
        }

        private string GetIconGuid(SkinConfig config)
        {
            var spriteSource = config.ContextData.SpriteSource;

            if (spriteSource == null)
            {
                return string.Empty;
            }

            return AtlasedSpriteReferences.GetSpriteGuid(spriteSource);
        }

        public void RefreshValueModel()
        {
            var model = new SkinValueModel(_globalSettingsConfig);
            var fingerprint = model.GetFingerprint();

            if (_valueModel != null && fingerprint == _valueModelFingerprint)
            {
                return;
            }

            _valueModel = model;
            _valueModelFingerprint = fingerprint;
            _distributions.Clear();
        }

        private class CachedDistribution
        {
            public readonly SkinValueDistribution Distribution;

            private readonly float _minPrice;
            private readonly float _maxPrice;
            private readonly bool _isStatTrakAvailable;
            private readonly bool _isSouvenirAvailable;
            private readonly int _modelFingerprint;

            public CachedDistribution(SkinConfig config, int modelFingerprint, SkinValueDistribution distribution)
            {
                Distribution = distribution;
                _minPrice = config.Price.MinPrice;
                _maxPrice = config.Price.MaxPrice;
                _isStatTrakAvailable = config.AvailableStatTrak;
                _isSouvenirAvailable = config.AvailableSouvenir;
                _modelFingerprint = modelFingerprint;
            }

            public bool IsActual(SkinConfig config, int modelFingerprint)
            {
                return _modelFingerprint == modelFingerprint
                       && _minPrice == config.Price.MinPrice
                       && _maxPrice == config.Price.MaxPrice
                       && _isStatTrakAvailable == config.AvailableStatTrak
                       && _isSouvenirAvailable == config.AvailableSouvenir;
            }
        }

        private class CachedIconPresence
        {
            public readonly string Guid;
            public readonly bool HasIcon;

            public CachedIconPresence(string guid, bool hasIcon)
            {
                Guid = guid;
                HasIcon = hasIcon;
            }
        }

        private class CachedIcon
        {
            public readonly string Guid;
            public readonly Sprite Sprite;

            public CachedIcon(string guid, Sprite sprite)
            {
                Guid = guid;
                Sprite = sprite;
            }
        }
    }
}

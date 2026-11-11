using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using QCore.Tools.Extensions;
using QCore.Tools.Identifiers;
using QCore.Tools.Mathematics;
using Features.Skins.Configs;
using Shared.Tools.Collections;
using Features.Skins.Data;
using Shared.Lifecycle;

namespace Features.Skins.Services
{
    public class SkinsDatabaseService : ISkinsDatabaseService, IInitializableService, IAssetsLoader
    {
        public IEnumerable<SkinConfig> AllSkinsConfigs => _skinsMap.Values;
        
        private readonly AllSkinsConfig _allSkinsConfig;
        private readonly SkinsGlobalSettingsConfig _skinsGlobalSettingsConfig;

        private Dictionary<QGuid, SkinConfig> _skinsMap;

        public SkinsDatabaseService(AllSkinsConfig allSkinsConfig, SkinsGlobalSettingsConfig skinsGlobalSettingsConfig)
        {
            _allSkinsConfig = allSkinsConfig;
            _skinsGlobalSettingsConfig = skinsGlobalSettingsConfig;
        }
        
        public async UniTask LoadAssets()
        {
            var tasks = new List<UniTask>(_allSkinsConfig.Skins.Count);

            foreach (var skin in _allSkinsConfig.Skins)
            {
                var task = skin.LoadAssets();
                tasks.Add(task);
            }

            await UniTask.WhenAll(tasks);
        }

        public void Init()
        {
            _skinsMap = _allSkinsConfig.Skins.ToDictionary(a => a.Id, b => b);
        }

        public SkinConfig GetSkinConfigById(QGuid id)
        {
            return _skinsMap[id];
        }

        public Wear GetSkinWear(Skin skin)
        {
            var floatValue = skin.FloatValue;

            foreach (var pair in _skinsGlobalSettingsConfig.WearValuesMap)
            {
                var range = pair.Value;
                
                if (range.InRange(floatValue))
                {
                    return pair.Key;
                }
            }

            return Wear.BattleScarred;
        }

        public Rarity GetSkinRarity(Skin skin)
        {
            var skinConfig = GetSkinConfigById(skin.SkinSourceId);
            return skinConfig.Rarity;
        }

        public SkinConfig GetRandomSkinConfig()
        {
            return _allSkinsConfig.Skins.GetRandomReadOnly();
        }

        public void ReleaseAssets()
        {
            foreach (var skin in _allSkinsConfig.Skins)
            {
                skin.ReleaseAssets();
            }
        }
    }
}
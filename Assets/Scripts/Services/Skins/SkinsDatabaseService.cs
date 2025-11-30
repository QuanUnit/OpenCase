using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using QCore.Tools.Identifiers;
using QCore.Tools.Mathematics;
using Services.Skins.Configs;
using Zenject;

namespace Services.Skins
{
    public class SkinsDatabaseService : ISkinsDatabaseService
    {
        public IEnumerable<SkinConfig> AllSkinsConfigs => _skinsMap.Values;
        
        [Inject] private AllSkinsConfig _allSkinsConfig;
        [Inject] private SkinsGlobalSettingsConfig _skinsGlobalSettingsConfig;

        private Dictionary<QGuid, SkinConfig> _skinsMap;
        
        public async UniTask LoadAllSkinsAssets()
        {
            List<UniTask> tasks = new List<UniTask>(_allSkinsConfig.Skins.Count);

            foreach (var skin in _allSkinsConfig.Skins)
            {
                UniTask task = skin.LoadAssets();
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
            float floatValue = skin.FloatValue;

            foreach (var pair in _skinsGlobalSettingsConfig.WearValuesMap)
            {
                FloatRange range = pair.Value;
                
                if (range.InRange(floatValue))
                {
                    return pair.Key;
                }
            }

            return Wear.BattleScarred;
        }

        public void ReleaseAllSkinsAssets()
        {
            foreach (var skin in _allSkinsConfig.Skins)
            {
                skin.ReleaseAssets();
            }
        }
    }
}
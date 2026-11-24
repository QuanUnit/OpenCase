using System.Collections.Generic;
using Features.Skins.Configs;
using Features.Skins.Data;
using Features.Skins.Services;

namespace Features.Contracts.Tests.EditMode
{
    public class FakeSkinsDatabaseService : ISkinsDatabaseService
    {
        public IEnumerable<SkinConfig> AllSkinsConfigs => _configs;

        private readonly List<SkinConfig> _configs = new List<SkinConfig>();

        public void Add(SkinConfig config)
        {
            _configs.Add(config);
        }

        public SkinConfig GetSkinConfigById(SkinId id)
        {
            return _configs.Find(config => config.Id == id);
        }

        public Wear GetSkinWear(Skin skin)
        {
            return default;
        }

        public Rarity GetSkinRarity(Skin skin)
        {
            return default;
        }

        public SkinConfig GetRandomSkinConfig()
        {
            return _configs[0];
        }
    }
}

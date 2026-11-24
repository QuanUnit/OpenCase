using System.Collections.Generic;
using Features.Skins.Configs;
using Features.Skins.Data;
using Features.Skins.Services;
using QCore.Tools.Identifiers;

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

        public SkinConfig GetSkinConfigById(QGuid id)
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

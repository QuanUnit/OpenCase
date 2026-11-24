using System.Collections.Generic;
using Features.Skins.Configs;
using Features.Skins.Data;

namespace Features.Skins.Services
{
    public interface ISkinsDatabaseService
    {
        public IEnumerable<SkinConfig> AllSkinsConfigs { get; }
        public SkinConfig GetSkinConfigById(SkinId id);
        public Wear GetSkinWear(Skin skin);
        public Rarity GetSkinRarity(Skin skin);
        public SkinConfig GetRandomSkinConfig();
    }
}
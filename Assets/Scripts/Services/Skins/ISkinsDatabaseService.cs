using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using QCore.Tools.Identifiers;
using Services.Skins.Configs;

namespace Services.Skins
{
    public interface ISkinsDatabaseService
    {
        public IEnumerable<SkinConfig> AllSkinsConfigs { get; }
        public UniTask LoadAllSkinsAssets();
        public void Init();
        public SkinConfig GetSkinConfigById(QGuid id);
        public Wear GetSkinWear(Skin skin);
        public void ReleaseAllSkinsAssets();
    }
}
using QCore.Tools.Identifiers;
using Services.Skins.Configs;

namespace Services.Skins
{
    public interface ISkinsDatabaseService
    {
        public void Init();
        public SkinConfig GetSkinConfigById(QGuid id);
        public Wear GetSkinWear(Skin skin);
    }
}
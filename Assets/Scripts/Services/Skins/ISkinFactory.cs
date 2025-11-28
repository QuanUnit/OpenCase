using Services.Skins.Configs;

namespace Services.Skins
{
    public interface ISkinFactory
    {
        public Skin CreateRandom(SkinConfig config);
    }
}
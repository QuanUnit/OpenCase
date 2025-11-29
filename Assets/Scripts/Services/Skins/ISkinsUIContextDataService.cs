using Services.Skins.Configs;

namespace Services.Skins
{
    public interface ISkinsUIContextDataService
    {
        public void Init();
        public RarityVisual GetRarityVisual(Rarity rarity);
    }
}
using Features.Skins.Configs;
using Features.Skins.Data;

namespace Features.Skins.Services
{
    public interface ISkinsUIContextDataService
    {
        public RarityVisual GetRarityVisual(Rarity rarity);
        public string GetWearName(Wear wear);
    }
}
using System.Collections.Generic;
using System.Linq;
using Features.Skins.Configs;
using Features.Skins.Data;
using Shared.Lifecycle;

namespace Features.Skins.Services
{
    public class SkinsUIContextDataService : ISkinsUIContextDataService, IInitializableService
    {
        private readonly SkinsUIContextDataConfig _skinsUIContextData;

        private Dictionary<Rarity, RarityVisual> _rarityVisualsMap;
        private Dictionary<Wear, string> _wearNamesMap;

        public SkinsUIContextDataService(SkinsUIContextDataConfig skinsUIContextData)
        {
            _skinsUIContextData = skinsUIContextData;
        }
        
        public void Init()
        {
            _rarityVisualsMap = _skinsUIContextData.RarityVisuals.ToDictionary(a => a.Key, b => b.Value);
            _wearNamesMap = _skinsUIContextData.WearNames.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public RarityVisual GetRarityVisual(Rarity rarity)
        {
            return _rarityVisualsMap[rarity];
        }

        public string GetWearName(Wear wear)
        {
            return _wearNamesMap[wear];
        }
    }
}
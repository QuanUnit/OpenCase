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

        public SkinsUIContextDataService(SkinsUIContextDataConfig skinsUIContextData)
        {
            _skinsUIContextData = skinsUIContextData;
        }
        
        public void Init()
        {
            _rarityVisualsMap = _skinsUIContextData.RarityVisuals.ToDictionary(a => a.Key, b => b.Value);
        }

        public RarityVisual GetRarityVisual(Rarity rarity)
        {
            return _rarityVisualsMap[rarity];
        }
    }
}
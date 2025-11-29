using System.Collections.Generic;
using System.Linq;
using Services.Skins.Configs;
using Zenject;

namespace Services.Skins
{
    public class SkinsUIContextDataService : ISkinsUIContextDataService
    {
        [Inject] private SkinsUIContextDataConfig _skinsUIContextData;

        private Dictionary<Rarity, RarityVisual> _rarityVisualsMap;
        
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
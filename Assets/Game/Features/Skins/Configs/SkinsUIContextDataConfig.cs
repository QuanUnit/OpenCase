using System.Collections.Generic;
using QCore.Tools.Collections;
using UnityEngine;
using Features.Skins.Data;

namespace Features.Skins.Configs
{
    [CreateAssetMenu(menuName = "Configs/Skins/" + nameof(SkinsUIContextDataConfig), fileName = nameof(SkinsUIContextDataConfig))]
    public class SkinsUIContextDataConfig : ScriptableObject
    {
        public IReadOnlyList<KeyValue<Rarity, RarityVisual>> RarityVisuals => _rarityVisuals;
        public IReadOnlyList<KeyValue<Wear, string>> WearNames => _wearNames;
        
        [SerializeField] private List<KeyValue<Rarity, RarityVisual>> _rarityVisuals;
        [SerializeField] private List<KeyValue<Wear, string>> _wearNames;
    }
}
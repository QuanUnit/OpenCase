using System.Collections.Generic;
using UnityEngine;

namespace Services.Skins.Configs
{
    [CreateAssetMenu(menuName = "Configs/Skins/" + nameof(AllSkinsConfig), fileName = nameof(AllSkinsConfig))]
    public class AllSkinsConfig : ScriptableObject
    {
        public IReadOnlyList<SkinConfig> Skins => _skins;
        
        [SerializeField] private List<SkinConfig> _skins;
    }
}
using System.Collections.Generic;
using NaughtyAttributes;
using Shared.Tools.AssetsExtensions;
using UnityEngine;

namespace Features.Skins.Configs
{
    [CreateAssetMenu(menuName = "Configs/Skins/" + nameof(AllSkinsConfig), fileName = nameof(AllSkinsConfig))]
    public class AllSkinsConfig : ScriptableObject
    {
        public IReadOnlyList<SkinConfig> Skins => _skins;
        
        [SerializeField] private List<SkinConfig> _skins;
        
#if UNITY_EDITOR
        
        [Button]
        public void Collect()
        {
            _skins = AssetDatabaseTools.FindAllAssets<SkinConfig>();
        }
        
#endif
    }
}
using System.Collections.Generic;
using NaughtyAttributes;
using Tools.AssetsExtensions;
using UnityEngine;

namespace Services.Cases.Configs
{
    [CreateAssetMenu(menuName = "Configs/Cases/" + nameof(AllCasesConfig), fileName = nameof(AllCasesConfig))]
    public class AllCasesConfig : ScriptableObject
    {
        public IReadOnlyList<CaseConfig> Cases => _cases;
        
        [SerializeField] private List<CaseConfig> _cases;

#if UNITY_EDITOR
        
        [Button]
        private void Collect()
        {
            _cases = AssetDatabaseTools.FindAllAssets<CaseConfig>();
        }
        
#endif

    }
}
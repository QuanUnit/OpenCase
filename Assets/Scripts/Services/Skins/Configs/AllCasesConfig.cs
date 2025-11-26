using System.Collections.Generic;
using UnityEngine;

namespace Services.Skins.Configs
{
    [CreateAssetMenu(menuName = "Configs/Skins/" + nameof(AllCasesConfig), fileName = nameof(AllCasesConfig))]
    public class AllCasesConfig : ScriptableObject
    {
        public IReadOnlyList<CaseConfig> Cases => _cases;
        
        [SerializeField] private List<CaseConfig> _cases;
    }
}
using System;
using System.Collections.Generic;
using QCore.Tools.Collections;
using UnityEngine;

namespace Features.Cases.Configs
{
    [CreateAssetMenu(menuName = "Configs/Cases/" + nameof(AllCasesConfig), fileName = nameof(AllCasesConfig))]
    public class AllCasesConfig : ScriptableObject
    {
        public IReadOnlyList<KeyValue<CasesCategory, List<CaseConfig>>> CategoriesMap => _categoriesMap;
        
        [SerializeField] private List<KeyValue<CasesCategory, List<CaseConfig>>> _categoriesMap;
    }
    
    [Serializable]
    public struct CasesCategory
    {
        public string Name;
    }
}
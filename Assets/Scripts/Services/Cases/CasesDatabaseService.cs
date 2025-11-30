using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using QCore.Tools.Collections;
using QCore.Tools.Identifiers;
using Services.Cases.Configs;
using Zenject;

namespace Services.Cases
{
    public class CasesDatabaseService : ICasesDatabaseService
    {
        [Inject] private AllCasesConfig _allCasesConfig;

        public IReadOnlyList<KeyValue<CasesCategory, List<CaseConfig>>> AllCases => _allCasesConfig.CategoriesMap;
        private Dictionary<QGuid, CaseConfig> _allCasesMap;

        public void Init()
        {
            _allCasesMap = _allCasesConfig.CategoriesMap.SelectMany(a => a.Value)
                .ToDictionary(a => a.Id, b => b);
        }
        
        public async UniTask LoadCasesAssets()
        {
            List<UniTask> tasks = new List<UniTask>();

            foreach (var pair in _allCasesConfig.CategoriesMap)
            {
                foreach (var caseConfig in pair.Value)
                {
                    tasks.Add(caseConfig.LoadAssets());
                }
            }

            await UniTask.WhenAll(tasks);
        }

        public CaseConfig GetCaseById(QGuid id)
        {
            return _allCasesMap[id];
        }

        public void ReleaseCasesAssets()
        {
            foreach (var pair in _allCasesConfig.CategoriesMap)
            {
                foreach (var caseConfig in pair.Value)
                {
                    caseConfig.ReleaseAssets();
                }
            }
        }
    }
}
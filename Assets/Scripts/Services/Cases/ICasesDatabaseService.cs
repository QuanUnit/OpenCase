using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using QCore.Tools.Collections;
using QCore.Tools.Identifiers;
using Services.Cases.Configs;

namespace Services.Cases
{
    public interface ICasesDatabaseService
    {
        public IReadOnlyList<KeyValue<CasesCategory, List<CaseConfig>>> AllCases { get; }

        public void Init();
        public UniTask LoadCasesAssets();
        public CaseConfig GetCaseById(QGuid id);
        public void ReleaseCasesAssets();
    }
}
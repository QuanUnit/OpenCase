using System.Collections.Generic;
using QCore.Tools.Collections;
using QCore.Tools.Identifiers;
using Features.Cases.Configs;

namespace Features.Cases.Services
{
    public interface ICasesDatabaseService
    {
        public IReadOnlyList<KeyValue<CasesCategory, List<CaseConfig>>> AllCases { get; }

        public CaseConfig GetCaseById(QGuid id);
    }
}
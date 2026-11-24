using System.Collections.Generic;
using QCore.Tools.Collections;
using Features.Cases.Configs;
using Features.Cases.Data;

namespace Features.Cases.Services
{
    public interface ICasesDatabaseService
    {
        public IReadOnlyList<KeyValue<CasesCategory, List<CaseConfig>>> AllCases { get; }

        public CaseConfig GetCaseById(CaseId id);
    }
}
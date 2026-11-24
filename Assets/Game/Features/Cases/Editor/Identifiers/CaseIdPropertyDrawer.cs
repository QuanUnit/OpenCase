using System.Collections.Generic;
using System.Linq;
using Features.Cases.Configs;
using Features.Cases.Data;
using Shared.Editor.Identifiers;
using Shared.Tools.AssetsExtensions;
using UnityEditor;

namespace Features.Cases.Editor.Identifiers
{
    [CustomPropertyDrawer(typeof(CaseId))]
    public class CaseIdPropertyDrawer : EntityIdPropertyDrawer<CaseId>
    {
        protected override IEnumerable<CaseId> GetIds()
        {
            return AssetDatabaseTools
                .FindAllAssets<CaseConfig>()
                .Select(config => config.Id);
        }
    }
}

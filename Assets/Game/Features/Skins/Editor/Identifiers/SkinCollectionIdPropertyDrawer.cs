using System.Collections.Generic;
using System.Linq;
using Features.Skins.Configs;
using Features.Skins.Data;
using Shared.Editor.Identifiers;
using Shared.Tools.AssetsExtensions;
using UnityEditor;

namespace Features.Skins.Editor.Identifiers
{
    [CustomPropertyDrawer(typeof(SkinCollectionId))]
    public class SkinCollectionIdPropertyDrawer : EntityIdPropertyDrawer<SkinCollectionId>
    {
        protected override IEnumerable<SkinCollectionId> GetIds()
        {
            return AssetDatabaseTools
                .FindAllAssets<SkinCollectionsConfig>()
                .SelectMany(config => config.Collections)
                .Select(entry => entry.Id);
        }
    }
}

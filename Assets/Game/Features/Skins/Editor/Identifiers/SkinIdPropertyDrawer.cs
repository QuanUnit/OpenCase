using System.Collections.Generic;
using System.Linq;
using Features.Skins.Configs;
using Features.Skins.Data;
using Shared.Editor.Identifiers;
using Shared.Tools.AssetsExtensions;
using UnityEditor;

namespace Features.Skins.Editor.Identifiers
{
    [CustomPropertyDrawer(typeof(SkinId))]
    public class SkinIdPropertyDrawer : EntityIdPropertyDrawer<SkinId>
    {
        private Dictionary<SkinId, string> _weaponNames;

        protected override IEnumerable<SkinId> GetIds()
        {
            var configs = AssetDatabaseTools.FindAllAssets<SkinConfig>();

            _weaponNames = new Dictionary<SkinId, string>();

            foreach (var config in configs)
            {
                _weaponNames[config.Id] = config.WeaponName;
            }

            return configs.Select(config => config.Id);
        }

        protected override string GetGroup(SkinId id)
        {
            return _weaponNames.TryGetValue(id, out var weaponName) == true ? weaponName : string.Empty;
        }
    }
}

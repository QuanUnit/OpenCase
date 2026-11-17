using System;
using System.Collections.Generic;
using System.Linq;
using Features.RemoteConfigs.Editor.Exporters.Skins;

namespace Features.RemoteConfigs.Editor.Icons
{
    public class SkinIconWeaponResolver
    {
        private const char IdSeparator = '_';

        public List<string> Resolve(IReadOnlyList<KeyValuePair<string, string>> idsWithNames)
        {
            var weapons = idsWithNames
                .Select(pair => SkinIds.GetWeapon(pair.Key, pair.Value))
                .ToList();

            var knownWeapons = new HashSet<string>();

            for (var index = 0; index < weapons.Count; index++)
            {
                if (string.Equals(weapons[index], idsWithNames[index].Key, StringComparison.Ordinal) == false)
                {
                    knownWeapons.Add(weapons[index]);
                }
            }

            var weaponsByLength = knownWeapons
                .OrderByDescending(weapon => weapon.Length)
                .ThenBy(weapon => weapon, StringComparer.Ordinal)
                .ToList();

            for (var index = 0; index < weapons.Count; index++)
            {
                var id = idsWithNames[index].Key;

                if (string.Equals(weapons[index], id, StringComparison.Ordinal) == false)
                {
                    continue;
                }

                var prefixWeapon = weaponsByLength.FirstOrDefault(weapon => id.StartsWith(weapon + IdSeparator, StringComparison.Ordinal) == true);

                if (prefixWeapon != null)
                {
                    weapons[index] = prefixWeapon;
                }
            }

            return weapons;
        }
    }
}

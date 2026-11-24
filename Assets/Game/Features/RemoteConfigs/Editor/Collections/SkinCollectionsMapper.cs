using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Exporters.Skins;

namespace Features.RemoteConfigs.Editor.Collections
{
    public class SkinCollectionsMapper
    {
        public Dictionary<string, string> Map(IReadOnlyList<SourceSkin> skins)
        {
            var collections = new Dictionary<string, string>();
            var exactIds = new HashSet<string>();

            foreach (var skin in skins)
            {
                if (skin.collections.Length == 0)
                {
                    continue;
                }

                var collectionName = skin.collections[0].name;
                var id = SkinIds.Create(skin.weapon.name, SkinIds.GetName(skin.name));

                if (string.IsNullOrEmpty(skin.phase) == true)
                {
                    collections[id] = collectionName;
                    exactIds.Add(id);
                    continue;
                }

                var phaseId = SkinIds.AddPhase(id, skin.phase);
                collections[phaseId] = collectionName;
                exactIds.Add(phaseId);

                if (exactIds.Contains(id) == false)
                {
                    collections[id] = collectionName;
                }
            }

            return collections;
        }
    }
}

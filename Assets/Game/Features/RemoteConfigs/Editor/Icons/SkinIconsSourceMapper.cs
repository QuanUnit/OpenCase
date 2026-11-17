using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Exporters.Skins;

namespace Features.RemoteConfigs.Editor.Icons
{
    public class SkinIconsSourceMapper
    {
        public Dictionary<string, SkinIconSource> Map(IReadOnlyList<SourceSkin> skins)
        {
            var sources = new Dictionary<string, SkinIconSource>();
            var exactIds = new HashSet<string>();

            foreach (var skin in skins)
            {
                var id = SkinIds.Create(skin.weapon.name, SkinIds.GetName(skin.name));

                if (string.IsNullOrEmpty(skin.phase) == true)
                {
                    AddExact(sources, exactIds, id, skin.image);
                    continue;
                }

                AddExact(sources, exactIds, SkinIds.AddPhase(id, skin.phase), skin.image);
                AddFallback(sources, exactIds, id, skin.image);
            }

            return sources;
        }

        private void AddExact(Dictionary<string, SkinIconSource> sources, HashSet<string> exactIds, string id, string imageUrl)
        {
            if (exactIds.Contains(id) == true && HasImage(sources[id]) == true)
            {
                return;
            }

            sources[id] = new SkinIconSource(id, imageUrl);
            exactIds.Add(id);
        }

        private void AddFallback(Dictionary<string, SkinIconSource> sources, HashSet<string> exactIds, string id, string imageUrl)
        {
            if (exactIds.Contains(id) == true)
            {
                return;
            }

            if (sources.TryGetValue(id, out var existing) == true && HasImage(existing) == true)
            {
                return;
            }

            sources[id] = new SkinIconSource(id, imageUrl);
        }

        private bool HasImage(SkinIconSource source)
        {
            return string.IsNullOrEmpty(source.ImageUrl) == false;
        }
    }
}

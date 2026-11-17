using System;
using System.Collections.Generic;
using System.Linq;

namespace Features.RemoteConfigs.Editor.Icons
{
    public class SkinIconFolderResolver
    {
        private readonly string _iconsRoot;

        public SkinIconFolderResolver(string iconsRoot)
        {
            _iconsRoot = iconsRoot.TrimEnd('/');
        }

        public Dictionary<string, string> Resolve(IReadOnlyList<SkinIconConfigState> configs)
        {
            var votesByWeapon = new Dictionary<string, Dictionary<string, int>>();

            foreach (var config in configs)
            {
                if (config.CurrentIconPath == null || SkinIconFileNames.IsIconOf(config.CurrentIconPath, config.Id) == false)
                {
                    continue;
                }

                if (votesByWeapon.TryGetValue(config.Weapon, out var votes) == false)
                {
                    votes = new Dictionary<string, int>();
                    votesByWeapon.Add(config.Weapon, votes);
                }

                var folder = GetFolder(config.CurrentIconPath);
                votes.TryGetValue(folder, out var count);
                votes[folder] = count + 1;
            }

            var folders = new Dictionary<string, string>();

            foreach (var config in configs)
            {
                if (folders.ContainsKey(config.Weapon) == true)
                {
                    continue;
                }

                if (votesByWeapon.TryGetValue(config.Weapon, out var votes) == true)
                {
                    folders.Add(config.Weapon, GetMostVoted(votes));
                }
                else
                {
                    folders.Add(config.Weapon, $"{_iconsRoot}/{SkinIconFileNames.ToFolderName(config.Weapon)}");
                }
            }

            return folders;
        }

        private string GetFolder(string path)
        {
            var normalizedPath = SkinIconFileNames.NormalizePath(path);
            var separatorIndex = normalizedPath.LastIndexOf('/');

            return separatorIndex < 0 ? string.Empty : normalizedPath.Substring(0, separatorIndex);
        }

        private string GetMostVoted(Dictionary<string, int> votes)
        {
            return votes
                .OrderByDescending(pair => pair.Value)
                .ThenBy(pair => pair.Key, StringComparer.Ordinal)
                .First()
                .Key;
        }
    }
}

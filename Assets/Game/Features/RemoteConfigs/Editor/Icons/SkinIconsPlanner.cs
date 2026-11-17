using System;
using System.Collections.Generic;
using System.Linq;

namespace Features.RemoteConfigs.Editor.Icons
{
    public class SkinIconsPlanner
    {
        public SkinIconsPlan Build(
            IReadOnlyList<SkinIconConfigState> configs,
            IReadOnlyDictionary<string, SkinIconSource> sources,
            SkinIconsMode mode,
            Func<string, string> folderByWeapon,
            Func<string, bool> fileExists)
        {
            var plan = new SkinIconsPlan(mode);
            var claimedTargets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var config in configs)
            {
                if (IsCandidate(config, mode) == false)
                {
                    plan.Skipped.Add(new SkinIconsSkippedItem(config.Id, SkinIconSkipReason.AlreadyOk));
                    continue;
                }

                if (sources.TryGetValue(config.Id, out var source) == false)
                {
                    plan.Skipped.Add(new SkinIconsSkippedItem(config.Id, SkinIconSkipReason.NoSource));
                    continue;
                }

                if (string.IsNullOrEmpty(source.ImageUrl) == true)
                {
                    plan.Skipped.Add(new SkinIconsSkippedItem(config.Id, SkinIconSkipReason.NoImage));
                    continue;
                }

                var folder = folderByWeapon(config.Weapon);

                if (string.IsNullOrEmpty(folder) == true)
                {
                    plan.Skipped.Add(new SkinIconsSkippedItem(config.Id, SkinIconSkipReason.NoFolder));
                    continue;
                }

                var targetPath = $"{folder}/{SkinIconFileNames.ToFileName(config.Id)}";

                if (config.CurrentIconPath != null && IsSamePath(config.CurrentIconPath, targetPath) == true)
                {
                    plan.Skipped.Add(new SkinIconsSkippedItem(config.Id, SkinIconSkipReason.AlreadyOk));
                    continue;
                }

                SkinIconAction action;

                if (claimedTargets.TryGetValue(targetPath, out var claimedId) == true)
                {
                    if (string.Equals(claimedId, config.Id, StringComparison.Ordinal) == false)
                    {
                        plan.Skipped.Add(new SkinIconsSkippedItem(config.Id, SkinIconSkipReason.SharedFile));
                        continue;
                    }

                    action = SkinIconAction.ReuseExisting;
                }
                else
                {
                    action = fileExists(targetPath) == true ? SkinIconAction.ReuseExisting : SkinIconAction.Download;
                    claimedTargets.Add(targetPath, config.Id);
                }

                plan.Items.Add(new SkinIconsPlanItem(config.Id, source.ImageUrl, targetPath, action, config.ConfigPath));
            }

            return plan;
        }

        public List<string> FindUnreferenced(IEnumerable<string> iconPaths, IEnumerable<string> referencedPaths)
        {
            var referenced = new HashSet<string>(referencedPaths.Select(SkinIconFileNames.NormalizePath), StringComparer.OrdinalIgnoreCase);

            return iconPaths
                .Where(path => referenced.Contains(SkinIconFileNames.NormalizePath(path)) == false)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();
        }

        private bool IsCandidate(SkinIconConfigState config, SkinIconsMode mode)
        {
            switch (mode)
            {
                case SkinIconsMode.OnlyMissing:
                    return config.CurrentIconPath == null;

                case SkinIconsMode.RepairMismatches:
                    return config.CurrentIconPath == null || SkinIconFileNames.IsIconOf(config.CurrentIconPath, config.Id) == false;

                case SkinIconsMode.ReplaceAll:
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        private bool IsSamePath(string first, string second)
        {
            return string.Equals(SkinIconFileNames.NormalizePath(first), SkinIconFileNames.NormalizePath(second), StringComparison.OrdinalIgnoreCase);
        }
    }
}

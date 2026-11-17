using System.Collections.Generic;
using System.Linq;

namespace Features.RemoteConfigs.Editor.Icons
{
    public class SkinIconsPlan
    {
        public readonly SkinIconsMode Mode;
        public readonly List<SkinIconsPlanItem> Items = new List<SkinIconsPlanItem>();
        public readonly List<SkinIconsSkippedItem> Skipped = new List<SkinIconsSkippedItem>();

        public int DownloadsCount => Items.Count(item => item.Action == SkinIconAction.Download);
        public int ReusesCount => Items.Count(item => item.Action == SkinIconAction.ReuseExisting);

        public SkinIconsPlan(SkinIconsMode mode)
        {
            Mode = mode;
        }

        public List<string> GetSkippedIds(SkinIconSkipReason reason)
        {
            return Skipped
                .Where(item => item.Reason == reason)
                .Select(item => item.Id)
                .ToList();
        }
    }

    public class SkinIconsPlanItem
    {
        public readonly string Id;
        public readonly string ImageUrl;
        public readonly string TargetPath;
        public readonly SkinIconAction Action;
        public readonly string ConfigPath;

        public SkinIconsPlanItem(string id, string imageUrl, string targetPath, SkinIconAction action, string configPath)
        {
            Id = id;
            ImageUrl = imageUrl;
            TargetPath = targetPath;
            Action = action;
            ConfigPath = configPath;
        }
    }

    public class SkinIconsSkippedItem
    {
        public readonly string Id;
        public readonly SkinIconSkipReason Reason;

        public SkinIconsSkippedItem(string id, SkinIconSkipReason reason)
        {
            Id = id;
            Reason = reason;
        }
    }
}

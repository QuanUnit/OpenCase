using System.Collections.Generic;
using System.Text;

namespace Features.RemoteConfigs.Editor.Icons
{
    public class SkinIconsReport
    {
        public readonly SkinIconsMode Mode;
        public readonly bool IsDryRun;
        public readonly int Planned;
        public readonly int PlannedDownloads;
        public readonly int PlannedReuses;
        public readonly int AlreadyOk;
        public readonly List<string> NoSource;
        public readonly List<string> NoImage;
        public readonly List<string> NoFolder;
        public readonly List<string> SharedFile;
        public readonly List<string> PlannedNewFolders = new List<string>();

        public readonly List<string> Downloaded = new List<string>();
        public readonly List<string> Reused = new List<string>();
        public readonly List<string> Assigned = new List<string>();
        public readonly List<string> Failed = new List<string>();
        public readonly List<string> CreatedFolders = new List<string>();
        public readonly List<string> UnreferencedIcons = new List<string>();
        public readonly List<string> Errors = new List<string>();

        public bool IsCancelled;
        public int RebuiltAtlases;

        public SkinIconsReport(SkinIconsPlan plan, bool isDryRun)
        {
            Mode = plan.Mode;
            IsDryRun = isDryRun;
            Planned = plan.Items.Count;
            PlannedDownloads = plan.DownloadsCount;
            PlannedReuses = plan.ReusesCount;
            AlreadyOk = plan.GetSkippedIds(SkinIconSkipReason.AlreadyOk).Count;
            NoSource = plan.GetSkippedIds(SkinIconSkipReason.NoSource);
            NoImage = plan.GetSkippedIds(SkinIconSkipReason.NoImage);
            NoFolder = plan.GetSkippedIds(SkinIconSkipReason.NoFolder);
            SharedFile = plan.GetSkippedIds(SkinIconSkipReason.SharedFile);
        }

        public bool HasProblems => NoSource.Count > 0 || NoImage.Count > 0 || NoFolder.Count > 0 || SharedFile.Count > 0 || Failed.Count > 0 || Errors.Count > 0;

        public string GetShortSummary()
        {
            var builder = new StringBuilder();
            var prefix = IsDryRun == true ? "DRY RUN, " : string.Empty;

            builder.AppendLine($"Skin icons ({prefix}{Mode})");

            if (IsCancelled == true)
            {
                builder.AppendLine("Cancelled: no project files were changed.");
            }

            builder.AppendLine($"Planned: {Planned} (download {PlannedDownloads}, reuse existing file {PlannedReuses})");

            if (IsDryRun == false)
            {
                builder.AppendLine($"Downloaded: {Downloaded.Count}, reused: {Reused.Count}, assigned: {Assigned.Count}, failed: {Failed.Count}");
            }

            builder.AppendLine($"Already ok: {AlreadyOk}");
            builder.AppendLine($"Skipped: no source {NoSource.Count}, no image {NoImage.Count}, no folder {NoFolder.Count}, shared file {SharedFile.Count}");
            builder.AppendLine($"New weapon folders: {(IsDryRun == true ? PlannedNewFolders.Count : CreatedFolders.Count)}");

            if (IsDryRun == false)
            {
                builder.AppendLine($"Sprite atlases rebuilt: {RebuiltAtlases}");
            }

            builder.Append($"Icon files not referenced by any config: {UnreferencedIcons.Count}");

            return builder.ToString();
        }

        public string GetFullSummary()
        {
            var builder = new StringBuilder();
            builder.AppendLine(GetShortSummary());

            AppendList(builder, "Errors", Errors);
            AppendList(builder, "Failed downloads", Failed);
            AppendList(builder, "Skipped, no source", NoSource);
            AppendList(builder, "Skipped, no image", NoImage);
            AppendList(builder, "Skipped, no folder", NoFolder);
            AppendList(builder, "Skipped, file name taken by another id", SharedFile);
            AppendList(builder, IsDryRun == true ? "Planned new weapon folders" : "Created weapon folders", IsDryRun == true ? PlannedNewFolders : CreatedFolders);
            AppendList(builder, "Icon files not referenced by any config (kept)", UnreferencedIcons);

            return builder.ToString();
        }

        private void AppendList(StringBuilder builder, string title, List<string> values)
        {
            if (values.Count == 0)
            {
                return;
            }

            builder.AppendLine();
            builder.AppendLine($"{title} ({values.Count}):");

            foreach (var value in values)
            {
                builder.AppendLine($"  {value}");
            }
        }
    }
}

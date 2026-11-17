using System.Collections.Generic;
using System.Text;

namespace Features.RemoteConfigs.Editor.Exporters.Skins
{
    public class SkinsSheetExportResult
    {
        public readonly SkinsSheetBuildResult BuildResult;
        public readonly SkinsSheetMergeResult MergeResult;
        public readonly List<List<string>> Values;
        public readonly ICollection<int> BooleanColumns;
        public readonly string Csv;

        public SkinsSheetExportResult(SkinsSheetBuildResult buildResult, SkinsSheetMergeResult mergeResult, List<List<string>> values, ICollection<int> booleanColumns, string csv)
        {
            BuildResult = buildResult;
            MergeResult = mergeResult;
            Values = values;
            BooleanColumns = booleanColumns;
            Csv = csv;
        }

        public string GetSummary()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Skins sheet: total {MergeResult.Rows.Count}, updated {MergeResult.UpdatedIds.Count}, added {MergeResult.AddedIds.Count}, kept not in source {MergeResult.KeptIds.Count}, kept skipped by builder {MergeResult.KeptSkippedIds.Count}, skipped without price {BuildResult.SkippedWithoutPrice.Count}, skipped unknown rarity {BuildResult.SkippedUnknownRarity.Count}");

            AppendList(builder, "Added", MergeResult.AddedIds);
            AppendList(builder, "Kept not in source", MergeResult.KeptIds);
            AppendList(builder, "Kept skipped by builder", MergeResult.KeptSkippedIds);
            AppendList(builder, "Skipped without price", BuildResult.SkippedWithoutPrice);
            AppendList(builder, "Skipped unknown rarity", BuildResult.SkippedUnknownRarity);

            return builder.ToString();
        }

        private void AppendList(StringBuilder builder, string title, List<string> items)
        {
            if (items.Count == 0)
            {
                return;
            }

            builder.AppendLine($"{title}: {string.Join(", ", items)}");
        }
    }
}

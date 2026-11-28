using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Features.RemoteConfigs.Editor.Interpreters
{
    public class MissingSkinsRemovalPolicy
    {
        private readonly float _maxMissingShare;

        public MissingSkinsRemovalPolicy(float maxMissingShare)
        {
            _maxMissingShare = maxMissingShare;
        }

        public bool CanOffer(ImportReport report)
        {
            if (report.CanRemoveMissing == false || report.Missing.Count == 0 || report.Errors.Count > 0)
            {
                return false;
            }

            var existingCount = report.Updated.Count + report.Missing.Count;

            return report.Missing.Count <= existingCount * _maxMissingShare;
        }

        public List<string> FindRemovable(IEnumerable<string> missingIds, ICollection<string> referencedIds)
        {
            return missingIds
                .Where(id => referencedIds.Contains(id) == false)
                .ToList();
        }

        public string BuildMessage(IReadOnlyList<string> removableIds, IReadOnlyList<string> referencedIds, int maxShown)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{removableIds.Count} skin configs are in the project but not in the sheet:");
            builder.AppendLine();
            AppendIds(builder, removableIds, maxShown);

            if (referencedIds.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine($"Kept {referencedIds.Count} skin configs still used by cases:");
                AppendIds(builder, referencedIds, maxShown);
            }

            builder.AppendLine();
            builder.Append("Delete them and remove them from AllSkinsConfig?");

            return builder.ToString();
        }

        private void AppendIds(StringBuilder builder, IReadOnlyList<string> ids, int maxShown)
        {
            foreach (var id in ids.Take(maxShown))
            {
                builder.AppendLine(id);
            }

            if (ids.Count > maxShown)
            {
                builder.AppendLine($"... and {ids.Count - maxShown} more (full list in the console)");
            }
        }
    }
}

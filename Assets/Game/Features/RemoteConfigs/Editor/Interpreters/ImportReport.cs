using System.Collections.Generic;
using System.Text;

namespace Features.RemoteConfigs.Editor.Interpreters
{
    public class ImportReport
    {
        public readonly List<string> Created = new List<string>();
        public readonly List<string> Updated = new List<string>();
        public readonly List<string> Missing = new List<string>();
        public readonly List<string> UnknownReferences = new List<string>();
        public readonly List<string> Errors = new List<string>();

        public string PageName;
        public bool CanRemoveMissing;
        public string ConfigsFolder;

        public bool HasProblems => Missing.Count > 0 || UnknownReferences.Count > 0 || Errors.Count > 0;

        public string GetShortSummary()
        {
            return $"{PageName}: created {Created.Count}, updated {Updated.Count}, not in sheet {Missing.Count}, unknown references {UnknownReferences.Count}, errors {Errors.Count}";
        }

        public string GetFullSummary()
        {
            var builder = new StringBuilder();
            builder.AppendLine(GetShortSummary());

            AppendList(builder, "Errors", Errors);
            AppendList(builder, "Created", Created);
            AppendList(builder, "Configs in project but not in sheet", Missing);
            AppendList(builder, "Unknown references (skipped)", UnknownReferences);

            return builder.ToString();
        }

        private void AppendList(StringBuilder builder, string title, List<string> values)
        {
            if (values.Count == 0)
            {
                return;
            }

            builder.AppendLine($"{title} ({values.Count}):");

            foreach (var value in values)
            {
                builder.AppendLine($"  {value}");
            }
        }
    }
}

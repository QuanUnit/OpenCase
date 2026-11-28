using System.Collections.Generic;

namespace Features.RemoteConfigs.Editor.Interpreters
{
    public class MissingSkinsRemovalResult
    {
        public readonly List<string> DeletedIds = new List<string>();
        public readonly List<string> FailedPaths = new List<string>();
    }
}

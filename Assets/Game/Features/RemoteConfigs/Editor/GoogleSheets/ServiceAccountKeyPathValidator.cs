using System;
using System.IO;

namespace Features.RemoteConfigs.Editor.GoogleSheets
{
    public class ServiceAccountKeyPathValidator
    {
        private readonly string _projectRoot;

        public ServiceAccountKeyPathValidator(string projectRoot)
        {
            _projectRoot = Path.GetFullPath(projectRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }

        public bool IsInsideProject(string keyPath)
        {
            var fullPath = Path.GetFullPath(keyPath);
            return fullPath.StartsWith(_projectRoot, StringComparison.OrdinalIgnoreCase) == true;
        }

        public void EnsureOutsideProject(string keyPath)
        {
            if (IsInsideProject(keyPath) == true)
            {
                throw new InvalidOperationException($"Service account key must be stored outside the project folder ({_projectRoot}) so it is never imported or committed. Move it and choose it again.");
            }
        }
    }
}

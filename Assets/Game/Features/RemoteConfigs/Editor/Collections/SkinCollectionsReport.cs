using System.Collections.Generic;

namespace Features.RemoteConfigs.Editor.Collections
{
    public class SkinCollectionsReport
    {
        public List<string> Assigned { get; } = new List<string>();
        public List<string> Unchanged { get; } = new List<string>();
        public List<string> Missing { get; } = new List<string>();
        public List<string> Registered { get; } = new List<string>();
    }
}

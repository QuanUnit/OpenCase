using System.Collections.Generic;

namespace Features.RemoteConfigs.Editor.Models
{
    public class CaseDataModel : IEntityModel
    {
        public string Id;
        public string Name;
        public float Price;
        public Dictionary<string, float> SkinsMap;
    }
}
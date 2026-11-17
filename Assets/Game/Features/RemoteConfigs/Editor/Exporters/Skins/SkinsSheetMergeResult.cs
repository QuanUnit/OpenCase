using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Models;

namespace Features.RemoteConfigs.Editor.Exporters.Skins
{
    public class SkinsSheetMergeResult
    {
        public readonly List<SkinDataModel> Rows = new List<SkinDataModel>();
        public readonly List<string> UpdatedIds = new List<string>();
        public readonly List<string> AddedIds = new List<string>();
        public readonly List<string> KeptIds = new List<string>();
        public readonly List<string> KeptSkippedIds = new List<string>();
    }
}

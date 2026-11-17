using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Models;

namespace Features.RemoteConfigs.Editor.Exporters.Skins
{
    public class SkinsSheetBuildResult
    {
        public readonly List<SkinDataModel> Rows = new List<SkinDataModel>();
        public readonly List<string> SkippedWithoutPrice = new List<string>();
        public readonly List<string> SkippedUnknownRarity = new List<string>();
        public readonly HashSet<string> SkippedIds = new HashSet<string>();
    }
}

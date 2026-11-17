using System.Collections.Generic;
using Features.Skins.Configs;

namespace Features.RemoteConfigs.Editor.Icons
{
    public class SkinIconsPreparation
    {
        public readonly SkinIconsPlan Plan;
        public readonly IReadOnlyList<SkinIconConfigState> States;
        public readonly IReadOnlyDictionary<string, string> FoldersByWeapon;
        public readonly IReadOnlyDictionary<string, SkinConfig> ConfigsByPath;

        public SkinIconsPreparation(SkinIconsPlan plan, IReadOnlyList<SkinIconConfigState> states, IReadOnlyDictionary<string, string> foldersByWeapon, IReadOnlyDictionary<string, SkinConfig> configsByPath)
        {
            Plan = plan;
            States = states;
            FoldersByWeapon = foldersByWeapon;
            ConfigsByPath = configsByPath;
        }
    }
}

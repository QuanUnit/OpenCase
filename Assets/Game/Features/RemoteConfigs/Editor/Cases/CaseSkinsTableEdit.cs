using Features.Cases.Configs;
using Features.Skins.Configs;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CaseSkinsTableEdit
    {
        public readonly CaseConfig Owner;
        public readonly SkinConfig Skin;
        public readonly bool IsRemoval;
        public readonly float Weight;

        public CaseSkinsTableEdit(CaseConfig owner, SkinConfig skin, bool isRemoval, float weight)
        {
            Owner = owner;
            Skin = skin;
            IsRemoval = isRemoval;
            Weight = weight;
        }
    }
}

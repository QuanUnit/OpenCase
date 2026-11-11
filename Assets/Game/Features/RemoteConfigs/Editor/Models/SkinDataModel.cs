using QCore.Tools.Mathematics;
using Features.Skins.Data;

namespace Features.RemoteConfigs.Editor.Models
{
    public class SkinDataModel : IEntityModel
    {
        public string Id;
        public string Name;
        public Rarity Rarity;
        public FloatRange Price;
        public bool AvailableStatTrak;
        public bool AvailableSouvenir;
    }
}
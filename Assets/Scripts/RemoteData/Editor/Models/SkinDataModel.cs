using QCore.Tools.Mathematics;
using Services.Skins;

namespace RemoteData.Editor.Models
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
using Services.Skins;
using Tools.Mathematics;

namespace RemoteData.Editor.Models
{
    public class SkinDataModel : IEntityModel
    {
        public string Id;
        public Rarity Rarity;
        public IntRange Price;
        public bool AvailableStatTrak;
        public bool AvailableSouvenir;
    }
}
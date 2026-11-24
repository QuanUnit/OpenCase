using Features.Skins.Data;

namespace Features.SlotMachine.Data
{
    public struct SlotData
    {
        public SkinId ItemId;

        public SlotData(SkinId itemId)
        {
            ItemId = itemId;
        }
    }
}
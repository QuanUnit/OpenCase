using QCore.Tools.Identifiers;

namespace SlotMachines.Horizontal
{
    public struct SlotData
    {
        public QGuid ItemId;

        public SlotData(QGuid itemId)
        {
            ItemId = itemId;
        }
    }
}
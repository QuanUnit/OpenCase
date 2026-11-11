using QCore.Tools.Identifiers;

namespace Features.SlotMachine.Data
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
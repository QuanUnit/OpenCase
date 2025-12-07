using QCore.Tools.Identifiers;

namespace Services.Skins
{
    public class Skin
    {
        public QGuid SkinSourceId { get; private set; }
        public QGuid Id { get; private set; }
        public float FloatValue { get; private set; }
        public bool IsStatTrak { get; private set; }
        public bool IsSouvenir { get; private set; }
        public float Price { get; private set; }
        
        public Skin(QGuid skinSourceId, QGuid id, float floatValue, float price, bool isStatTrak, bool isSouvenir)
        {
            Id = id;
            SkinSourceId = skinSourceId;
            FloatValue = floatValue;
            Price = price;
            IsStatTrak = isStatTrak;
            IsSouvenir = isSouvenir;
        }
    }
}
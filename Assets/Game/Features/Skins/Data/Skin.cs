namespace Features.Skins.Data
{
    public class Skin
    {
        public SkinId ConfigId { get; private set; }
        public SkinInstanceId Id { get; private set; }
        public float FloatValue { get; private set; }
        public bool IsStatTrak { get; private set; }
        public bool IsSouvenir { get; private set; }
        public float Price { get; private set; }
        
        public Skin(SkinId configId, SkinInstanceId id, float floatValue, float price, bool isStatTrak, bool isSouvenir)
        {
            Id = id;
            ConfigId = configId;
            FloatValue = floatValue;
            Price = price;
            IsStatTrak = isStatTrak;
            IsSouvenir = isSouvenir;
        }
    }
}
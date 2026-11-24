using System.Collections.Generic;
using Features.Skins.Data;

namespace Features.Inventory.Services
{
    public interface ISkinSellService
    {
        public float GetSellPrice(Skin skin);
        public float GetSellPrice(IReadOnlyList<Skin> skins);
        public bool CanSell(Skin skin);
        public float Sell(Skin skin);
        public float SellAll(IReadOnlyList<Skin> skins);
    }
}

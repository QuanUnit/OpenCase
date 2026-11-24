using System.Collections.Generic;
using Features.Skins.Data;
using Features.Wallet.Services;
using Shared.Tools.Mathematics;

namespace Features.Inventory.Services
{
    public class SkinSellService : ISkinSellService
    {
        private readonly IInventoryService _inventoryService;
        private readonly SoftCurrencyWallet _softCurrencyWallet;

        public SkinSellService(IInventoryService inventoryService, SoftCurrencyWallet softCurrencyWallet)
        {
            _inventoryService = inventoryService;
            _softCurrencyWallet = softCurrencyWallet;
        }

        public float GetSellPrice(Skin skin)
        {
            return MoneyMath.RoundToMoney(skin.Price);
        }

        public float GetSellPrice(IReadOnlyList<Skin> skins)
        {
            var total = 0f;

            foreach (var skin in skins)
            {
                total += GetSellPrice(skin);
            }

            return MoneyMath.RoundToMoney(total);
        }

        public bool CanSell(Skin skin)
        {
            return _inventoryService.TryGet(skin.Id, out _);
        }

        public float Sell(Skin skin)
        {
            if (_inventoryService.Remove(skin) == false)
            {
                return 0f;
            }

            var amount = GetSellPrice(skin);

            AddToWallet(amount);

            return amount;
        }

        public float SellAll(IReadOnlyList<Skin> skins)
        {
            var soldSkins = new List<Skin>(skins.Count);

            foreach (var skin in skins)
            {
                if (_inventoryService.Remove(skin) == true)
                {
                    soldSkins.Add(skin);
                }
            }

            var amount = GetSellPrice(soldSkins);

            AddToWallet(amount);

            return amount;
        }

        private void AddToWallet(float amount)
        {
            if (amount > 0f)
            {
                _softCurrencyWallet.Add(amount);
            }
        }
    }
}

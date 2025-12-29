using GameHub.Inventory.Panels;
using QCore.Tools.Identifiers;
using QCore.UISystem.Panels;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services;
using Services.Inventory;
using Services.Skins;
using Services.Skins.Configs;
using Services.Wallet;
using Zenject;

namespace GameHub.Inventory.Presenters
{
    public class SkinDetailsPopupPresenter : PanelPresenter<SkinDetailsPopup>, IPanelPresenterWithPayload<QGuid>
    {
        [Inject] private IInventoryService _inventoryService;
        [Inject] private ISkinsUIContextDataService _skinsUIContextDataService;
        [Inject] private ISkinsDatabaseService _skinsDatabaseService;
        [Inject] private IUIPanelsService _uiPanelsService;
        [Inject] private SoftCurrencyWallet _wallet;
        
        private Skin _skin;
        
        public void SetPayload(QGuid payload)
        {
            _skin = _inventoryService.Get(payload);
        }
        
        protected override void StartPresentingInternal()
        {
            Rarity rarity = _skinsDatabaseService.GetSkinRarity(_skin);
            SkinConfig skinConfig = _skinsDatabaseService.GetSkinConfigById(_skin.SkinSourceId);
            RarityVisual rarityVisual = _skinsUIContextDataService.GetRarityVisual(rarity);

            Panel.CloseClicked += CloseClickHandle;
            Panel.SellClicked += SellClicked;
            
            Panel.SetView(new SkinDetailsPopup.Data()
            {
                Price = _skin.Price,
                RarityColor = rarityVisual.RarityMainColor,
                SkinIcon = skinConfig.ContextData.SpriteSource.GetAsset(),
                SkinName = skinConfig.Name,
                FloatValue = _skin.FloatValue,
            });
        }

        private void CloseClickHandle()
        {
            _uiPanelsService.Close<SkinDetailsPopup>();
        }
        
        private void SellClicked()
        {
            _uiPanelsService.Close<SkinDetailsPopup>();

            float price = _skin.Price;
            
            _wallet.Add(price);
            _inventoryService.Remove(_skin);
        }

        protected override void StopPresentingInternal()
        {
            Panel.CloseClicked -= CloseClickHandle;
            Panel.SellClicked -= SellClicked;
        }
    }
}
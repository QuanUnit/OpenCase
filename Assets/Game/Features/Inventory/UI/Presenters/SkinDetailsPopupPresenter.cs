using Features.Inventory.UI.Panels;
using QCore.Tools.Identifiers;
using QCore.UISystem.Panels;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services;
using Features.Inventory.Services;
using Features.Skins.Data;
using Features.Skins.Services;
using Features.Skins.Configs;
using Features.Wallet.Services;

namespace Features.Inventory.UI.Presenters
{
    public class SkinDetailsPopupPresenter : PanelPresenter<SkinDetailsPopup>, IPanelPresenterWithPayload<QGuid>
    {
        private readonly IInventoryService _inventoryService;
        private readonly ISkinsUIContextDataService _skinsUIContextDataService;
        private readonly ISkinsDatabaseService _skinsDatabaseService;
        private readonly IUIPanelsService _uiPanelsService;
        private readonly SoftCurrencyWallet _wallet;
        
        private Skin _skin;

        public SkinDetailsPopupPresenter(IInventoryService inventoryService, ISkinsUIContextDataService skinsUIContextDataService, ISkinsDatabaseService skinsDatabaseService, IUIPanelsService uiPanelsService, SoftCurrencyWallet wallet)
        {
            _inventoryService = inventoryService;
            _skinsUIContextDataService = skinsUIContextDataService;
            _skinsDatabaseService = skinsDatabaseService;
            _uiPanelsService = uiPanelsService;
            _wallet = wallet;
        }
        
        public void SetPayload(QGuid payload)
        {
            _skin = _inventoryService.Get(payload);
        }
        
        protected override void StartPresentingInternal()
        {
            var rarity = _skinsDatabaseService.GetSkinRarity(_skin);
            var skinConfig = _skinsDatabaseService.GetSkinConfigById(_skin.SkinSourceId);
            var rarityVisual = _skinsUIContextDataService.GetRarityVisual(rarity);

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

            var price = _skin.Price;
            
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
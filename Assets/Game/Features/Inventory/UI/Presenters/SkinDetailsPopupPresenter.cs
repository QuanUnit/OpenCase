using Features.Inventory.UI.Panels;
using QCore.Tools.Identifiers;
using QCore.UISystem.Panels;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services;
using Features.Inventory.Services;
using Features.Skins.Data;
using Features.Skins.Services;
using Features.Skins.Configs;

namespace Features.Inventory.UI.Presenters
{
    public class SkinDetailsPopupPresenter : PanelPresenter<SkinDetailsPopup>, IPanelPresenterWithPayload<QGuid>
    {
        private readonly IInventoryService _inventoryService;
        private readonly ISkinsUIContextDataService _skinsUIContextDataService;
        private readonly ISkinsDatabaseService _skinsDatabaseService;
        private readonly IUIPanelsService _uiPanelsService;
        private readonly ISkinSellService _skinSellService;
        
        private Skin _skin;

        public SkinDetailsPopupPresenter(IInventoryService inventoryService, ISkinsUIContextDataService skinsUIContextDataService, ISkinsDatabaseService skinsDatabaseService, IUIPanelsService uiPanelsService, ISkinSellService skinSellService)
        {
            _inventoryService = inventoryService;
            _skinsUIContextDataService = skinsUIContextDataService;
            _skinsDatabaseService = skinsDatabaseService;
            _uiPanelsService = uiPanelsService;
            _skinSellService = skinSellService;
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
                Price = _skinSellService.GetSellPrice(_skin),
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

            _skinSellService.Sell(_skin);
        }

        protected override void StopPresentingInternal()
        {
            Panel.CloseClicked -= CloseClickHandle;
            Panel.SellClicked -= SellClicked;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Shared.UI.SelectableSet;
using Features.Inventory.UI.Panels;
using Features.Skins.Data;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services;
using Features.Inventory.Services;
using Features.Skins.Services;
using Features.Skins.Configs;
using UnityEngine;
using Features.Inventory.Data;

namespace Features.Inventory.UI.Presenters
{
    public class InventorySubPanelPresenter : PanelPresenter<InventorySubPanel>
    {
        private readonly IInventoryService _inventoryService;
        private readonly ISkinsDatabaseService _skinsDatabaseService;
        private readonly ISkinsUIContextDataService _skinsUIContextDataService;
        private readonly IUIPanelsService _uiPanelsService;
        private readonly ISkinSellService _skinSellService;
        
        private SelectableSetController _controller;
        private SelectedSkinsBuffer _selectedSkinsBuffer;
        
        public InventorySubPanelPresenter(IInventoryService inventoryService, ISkinsDatabaseService skinsDatabaseService, ISkinsUIContextDataService skinsUIContextDataService, IUIPanelsService uiPanelsService, ISkinSellService skinSellService)
        {
            _inventoryService = inventoryService;
            _skinsDatabaseService = skinsDatabaseService;
            _skinsUIContextDataService = skinsUIContextDataService;
            _uiPanelsService = uiPanelsService;
            _skinSellService = skinSellService;

            _controller = new SelectableSetController();
            _selectedSkinsBuffer = new SelectedSkinsBuffer();
        }
        
        protected override void StartPresentingInternal()
        {
            var skins = _inventoryService.AllSkins;

            Panel.SkinClicked += SkinClickHandle;
            Panel.SellClicked += SellClickHandle;
            Panel.AddSkinViews(skins, CreateSkinView);

            _inventoryService.Removed += SkinRemoveHandle;
            _inventoryService.Added += SkinAddHandle;

            var controlBlock = Panel.SelectableSetControlBlock;
            
            _controller.SelectingModeChanged += SelectingModeChangeHandle;
            _controller.SelectionChanged += SelectionChangeHandle;
            _controller.Init(new SelectableSetController.Data()
            {
                DeselectAllButton = controlBlock.DeselectAllButton,
                SelectAllButton = controlBlock.SelectAllButton,
                StartSelectingButton = controlBlock.StartSelectingButton,
                StopSelectingButton = controlBlock.StopSelectingButton,
                SelectableItemsGetter = GetSelectableItems,
            });
        }

        private void SelectionChangeHandle(SelectableItem item)
        {
            var isSelected = item.IsSelected;

            var skinView = item.GetComponent<SkinView>();
            var skinId = Panel.GetSkinId(skinView);
            var skin = _inventoryService.Get(skinId);
            
            if (isSelected)
            {
                _selectedSkinsBuffer.AddSelected(skin);
            }
            else
            {
                _selectedSkinsBuffer.RemoveSelected(skin);
            }
            
            UpdateSellButtonCurrency();
        }

        private void SelectingModeChangeHandle(bool isSelectingMode)
        {
            if (isSelectingMode == false)
            {
                _selectedSkinsBuffer.Clear();
            }
            
            Panel.SwitchEnableSellButton(isSelectingMode);
            UpdateSellButtonCurrency();
        }

        private void UpdateSellButtonCurrency()
        {
            var totalCurrency = _skinSellService.GetSellPrice(_selectedSkinsBuffer.Skins.ToList());
            Panel.SetSellButtonPrice(totalCurrency);
        }

        private IEnumerable<GameObject> GetSelectableItems()
        {
            return Panel.ActiveViews.Select(a => a.gameObject);
        }

        private void SkinAddHandle(Skin skin)
        {
            Panel.AddSkinView(skin, CreateSkinView);
        }

        private void SkinRemoveHandle(Skin skin)
        {
            Panel.RemoveSkinView(skin.Id);
        }

        private void SkinClickHandle(SkinInstanceId skinId)
        {
            _uiPanelsService.OpenWithPayload(() => Panel.SkinDetailsPopup, skinId);
        }
        
        private void SellClickHandle()
        {
            _skinSellService.SellAll(_selectedSkinsBuffer.Skins.ToList());
            _selectedSkinsBuffer.Clear();
            _controller.StopSelecting();
        }

        private SkinView CreateSkinView(Skin skin, SkinView prefab, Transform parent)
        {
            var view = Object.Instantiate(prefab, parent);
            var skinConfig = _skinsDatabaseService.GetSkinConfigById(skin.ConfigId);
            var rarityVisual = _skinsUIContextDataService.GetRarityVisual(skinConfig.Rarity);
            
            view.SetView(new SkinView.Data()
            {
                Icon = skinConfig.ContextData.SpriteSource.GetAsset(),
                Name = skinConfig.Name,
                Price = _skinSellService.GetSellPrice(skin),
                RarityColor = rarityVisual.RarityMainColor,
            });

            return view;
        }

        protected override void StopPresentingInternal()
        {
            Panel.SkinClicked -= SkinClickHandle;
            Panel.SellClicked -= SellClickHandle;

            _inventoryService.Removed -= SkinRemoveHandle;
            _inventoryService.Added -= SkinAddHandle;
            
            Panel.RemoveAllSkinViews();
            _uiPanelsService.Close<SkinDetailsPopup>();
            
            _controller.SelectionChanged -= SelectionChangeHandle;
            _controller.SelectingModeChanged -= SelectingModeChangeHandle;
            _controller.Dispose();

            SelectingModeChangeHandle(false);
        }
    }
}
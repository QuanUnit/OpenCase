using System.Collections.Generic;
using System.Linq;
using Common.UI.SelectableSet;
using GameHub.Inventory.Panels;
using QCore.Tools.Identifiers;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services;
using Services.Inventory;
using Services.Skins;
using Services.Skins.Configs;
using Services.Wallet;
using UnityEngine;
using Zenject;

namespace GameHub.Inventory.Presenters
{
    public class InventorySubPanelPresenter : PanelPresenter<InventorySubPanel>
    {
        [Inject] private IInventoryService _inventoryService;
        [Inject] private ISkinsDatabaseService _skinsDatabaseService;
        [Inject] private ISkinsUIContextDataService _skinsUIContextDataService;
        [Inject] private IUIPanelsService _uiPanelsService;
        [Inject] private SoftCurrencyWallet _softCurrencyWallet;
        
        private SelectableSetController _controller;
        private SelectedSkinsBuffer _selectedSkinsBuffer;
        
        public InventorySubPanelPresenter()
        {
            _controller = new SelectableSetController();
            _selectedSkinsBuffer = new SelectedSkinsBuffer();
        }
        
        protected override void StartPresentingInternal()
        {
            IEnumerable<Skin> skins = _inventoryService.AllSkins;

            Panel.SkinClicked += SkinClickHandle;
            Panel.SellClicked += SellClickHandle;
            Panel.AddSkinViews(skins, CreateSkinView);

            _inventoryService.Removed += SkinRemoveHandle;
            _inventoryService.Added += SkinAddHandle;

            SelectableSetControlBlock controlBlock = Panel.SelectableSetControlBlock;
            
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
            bool isSelected = item.IsSelected;

            SkinView skinView = item.GetComponent<SkinView>();
            QGuid skinId = Panel.GetSkinId(skinView);
            Skin skin = _inventoryService.Get(skinId);
            
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
            float totalCurrency = _selectedSkinsBuffer.TotalPrice;
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

        private void SkinClickHandle(QGuid skinId)
        {
            _uiPanelsService.OpenWithPayload(() => Panel.SkinDetailsPopup, skinId);
        }
        
        private void SellClickHandle()
        {
            foreach (var skin in _selectedSkinsBuffer.Skins)
            {
                _inventoryService.Remove(skin);
            }
            
            _softCurrencyWallet.Add(_selectedSkinsBuffer.TotalPrice);
            _selectedSkinsBuffer.Clear();
            _controller.StopSelecting();
        }

        private SkinView CreateSkinView(Skin skin, SkinView prefab, Transform parent)
        {
            SkinView view = Object.Instantiate(prefab, parent);
            SkinConfig skinConfig = _skinsDatabaseService.GetSkinConfigById(skin.SkinSourceId);
            RarityVisual rarityVisual = _skinsUIContextDataService.GetRarityVisual(skinConfig.Rarity);
            
            view.SetView(new SkinView.Data()
            {
                Icon = skinConfig.ContextData.SpriteSource.GetAsset(),
                Name = skinConfig.Name,
                Price = skin.Price,
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
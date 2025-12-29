using System.Collections.Generic;
using GameHub.Inventory.Panels;
using QCore.Tools.Identifiers;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services;
using Services.Inventory;
using Services.Skins;
using Services.Skins.Configs;
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
        
        protected override void StartPresentingInternal()
        {
            IEnumerable<Skin> skins = _inventoryService.AllSkins;

            Panel.SkinClicked += SkinClickHandle;
            Panel.AddSkinViews(skins, CreateSkinView);

            _inventoryService.Removed += SkinRemoveHandle;
            _inventoryService.Added += SkinAddHandle;
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
            _inventoryService.Removed -= SkinRemoveHandle;
            _inventoryService.Added -= SkinAddHandle;
            
            Panel.RemoveAllSkinViews();
            _uiPanelsService.Close<SkinDetailsPopup>();
        }
    }
}
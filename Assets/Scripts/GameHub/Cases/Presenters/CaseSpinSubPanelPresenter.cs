using System.Linq;
using GameHub.Cases.Panels;
using QCore.Tools.Extensions;
using QCore.Tools.Identifiers;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services;
using Services.Cases;
using Services.Cases.Configs;
using Services.Inventory;
using Services.Skins;
using Services.Skins.Configs;
using Services.Wallet;
using SlotMachines.Horizontal;
using UnityEngine;
using Zenject;

namespace GameHub.Cases.Presenters
{
    public class CaseSpinSubPanelPresenter : PanelPresenter<CaseSpinSubPanel>, IPanelPresenterWithPayload<QGuid>
    {
        [Inject] private ICasesDatabaseService _casesDatabaseService;
        [Inject] private IUIPanelsService _panelsService;
        [Inject] private ISkinsDatabaseService _skinsDatabaseService;
        [Inject] private ISkinsUIContextDataService _skinsUIContextDataService;
        [Inject] private IInventoryService _inventoryService;
        [Inject] private ISkinFactory _skinFactory;
        [Inject] private SoftCurrencyWallet _softCurrencyWallet;
        
        private CaseConfig _caseConfig;
        
        protected override void StartPresentingInternal()
        {
            Panel.SpinClicked += SpinClickHandle;
            Panel.BackClicked += BackClickHandle;
        }
        
        public void SetPayload(QGuid payload)
        {
            _caseConfig = _casesDatabaseService.GetCaseById(payload);
        }

        private void BackClickHandle()
        {
            _panelsService.Close(Panel.GetType());
        }

        private void SpinClickHandle()
        {
            if(CanSpinCase() == false) return;
            
            HorizontalSlotMachine slotMachine = Panel.SlotMachine;

            if (slotMachine.IsSpinning) return;
            
            slotMachine.StopSpinAndClear();

            SlotData winSlot = DefineWinSlotData();
            slotMachine.Spin(winSlot, DefineSecondarySlotData, CreateSlot);

            Skin skin = CreateSkin(winSlot);
            
            _inventoryService.Add(skin);
            _softCurrencyWallet.Spend(_caseConfig.Price);
        }

        private bool CanSpinCase()
        {
            return _softCurrencyWallet.IsEnoughForSpend(_caseConfig.Price);
        }

        private Skin CreateSkin(SlotData slot)
        {
            SkinConfig skinConfig = _skinsDatabaseService.GetSkinConfigById(slot.ItemId);
            Skin skin = _skinFactory.CreateRandom(skinConfig);

            return skin;
        }

        private SlotData DefineWinSlotData()
        {
            SkinConfig winSkin = _caseConfig.SkinsMapEntries.GetRandom();
            return new SlotData(winSkin.Id);
        }

        private SlotData DefineSecondarySlotData()
        {
            SkinConfig winSkin = _caseConfig.SkinsMapEntries.GetRandom();
            return new SlotData(winSkin.Id);
        }
        
        private Slot CreateSlot(Slot prefab, SlotData slotData, Transform viewport)
        {
            QGuid skinId = slotData.ItemId;
            SkinConfig skinConfig = _skinsDatabaseService.GetSkinConfigById(skinId);

            SkinSlot slot = (SkinSlot)Object.Instantiate(prefab, viewport);

            Sprite skinSprite = skinConfig.ContextData.SpriteSource.GetAsset();
            RarityVisual rarityVisual = _skinsUIContextDataService.GetRarityVisual(skinConfig.Rarity);
            
            slot.SetData(skinSprite, rarityVisual);

            return slot;
        }

        protected override void StopPresentingInternal()
        {
            Panel.SpinClicked -= SpinClickHandle;
            Panel.BackClicked -= BackClickHandle;
            
            Panel.SlotMachine.StopSpinAndClear();
        }
    }
}
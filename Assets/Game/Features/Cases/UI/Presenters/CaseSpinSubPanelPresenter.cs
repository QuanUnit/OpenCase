using System.Linq;
using Features.Cases.UI.Panels;
using QCore.Tools.Extensions;
using QCore.Tools.Identifiers;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services;
using Features.Cases.Services;
using Features.Cases.Configs;
using Features.Inventory.Services;
using Features.Skins.Data;
using Features.Skins.Services;
using Features.Skins.Configs;
using Features.Wallet.Services;
using Features.SlotMachine.UI;
using Features.SlotMachine.Data;
using UnityEngine;

namespace Features.Cases.UI.Presenters
{
    public class CaseSpinSubPanelPresenter : PanelPresenter<CaseSpinSubPanel>, IPanelPresenterWithPayload<QGuid>
    {
        private readonly ICasesDatabaseService _casesDatabaseService;
        private readonly IUIPanelsService _panelsService;
        private readonly ISkinsDatabaseService _skinsDatabaseService;
        private readonly ISkinsUIContextDataService _skinsUIContextDataService;
        private readonly IInventoryService _inventoryService;
        private readonly ISkinFactory _skinFactory;
        private readonly SoftCurrencyWallet _softCurrencyWallet;
        
        private CaseConfig _caseConfig;

        public CaseSpinSubPanelPresenter(ICasesDatabaseService casesDatabaseService, IUIPanelsService panelsService, ISkinsDatabaseService skinsDatabaseService, ISkinsUIContextDataService skinsUIContextDataService, IInventoryService inventoryService, ISkinFactory skinFactory, SoftCurrencyWallet softCurrencyWallet)
        {
            _casesDatabaseService = casesDatabaseService;
            _panelsService = panelsService;
            _skinsDatabaseService = skinsDatabaseService;
            _skinsUIContextDataService = skinsUIContextDataService;
            _inventoryService = inventoryService;
            _skinFactory = skinFactory;
            _softCurrencyWallet = softCurrencyWallet;
        }
        
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
            
            var slotMachine = Panel.SlotMachine;

            if (slotMachine.IsSpinning) return;
            
            slotMachine.StopSpinAndClear();

            var winSlot = DefineWinSlotData();
            slotMachine.Spin(winSlot, DefineSecondarySlotData, CreateSlot);

            var skin = CreateSkin(winSlot);
            
            _inventoryService.Add(skin);
            _softCurrencyWallet.Spend(_caseConfig.Price);
        }

        private bool CanSpinCase()
        {
            return _softCurrencyWallet.IsEnoughForSpend(_caseConfig.Price);
        }

        private Skin CreateSkin(SlotData slot)
        {
            var skinConfig = _skinsDatabaseService.GetSkinConfigById(slot.ItemId);
            var skin = _skinFactory.CreateRandom(skinConfig);

            return skin;
        }

        private SlotData DefineWinSlotData()
        {
            var winSkin = _caseConfig.SkinsMapEntries.GetRandom();
            return new SlotData(winSkin.Id);
        }

        private SlotData DefineSecondarySlotData()
        {
            var winSkin = _caseConfig.SkinsMapEntries.GetRandom();
            return new SlotData(winSkin.Id);
        }
        
        private Slot CreateSlot(Slot prefab, SlotData slotData, Transform viewport)
        {
            var skinId = slotData.ItemId;
            var skinConfig = _skinsDatabaseService.GetSkinConfigById(skinId);

            var slot = (SkinSlot)Object.Instantiate(prefab, viewport);

            var skinSprite = skinConfig.ContextData.SpriteSource.GetAsset();
            var rarityVisual = _skinsUIContextDataService.GetRarityVisual(skinConfig.Rarity);
            
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
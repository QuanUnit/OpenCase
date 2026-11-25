using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Features.Cases.Configs;
using Features.Cases.Data;
using Features.Cases.Services;
using Features.Cases.UI.Panels;
using Features.Inventory.Services;
using Features.Skins.Data;
using Features.Skins.Services;
using Features.SlotMachine.Data;
using Features.SlotMachine.UI;
using Features.Wallet.Services;
using QCore.Tools.Extensions;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Features.Cases.UI.Presenters
{
    public class CaseSpinSubPanelPresenter : PanelPresenter<CaseSpinSubPanel>, IPanelPresenterWithPayload<CaseId>
    {
        private readonly ICasesDatabaseService _casesDatabaseService;
        private readonly IUIPanelsService _panelsService;
        private readonly ISkinsDatabaseService _skinsDatabaseService;
        private readonly ISkinsUIContextDataService _skinsUIContextDataService;
        private readonly IInventoryService _inventoryService;
        private readonly ISkinSellService _skinSellService;
        private readonly ISkinFactory _skinFactory;
        private readonly SoftCurrencyWallet _softCurrencyWallet;
        private readonly CaseSpinConfig _caseSpinConfig;
        private readonly CaseDropsResult _dropsResult = new CaseDropsResult();

        private CaseConfig _caseConfig;
        private bool _isPresenting;
        private bool _isSpinning;
        private int _spinRunId;
        private int _count;

        public CaseSpinSubPanelPresenter(ICasesDatabaseService casesDatabaseService, IUIPanelsService panelsService, ISkinsDatabaseService skinsDatabaseService, ISkinsUIContextDataService skinsUIContextDataService, IInventoryService inventoryService, ISkinSellService skinSellService, ISkinFactory skinFactory, SoftCurrencyWallet softCurrencyWallet, CaseSpinConfig caseSpinConfig)
        {
            _casesDatabaseService = casesDatabaseService;
            _panelsService = panelsService;
            _skinsDatabaseService = skinsDatabaseService;
            _skinsUIContextDataService = skinsUIContextDataService;
            _inventoryService = inventoryService;
            _skinSellService = skinSellService;
            _skinFactory = skinFactory;
            _softCurrencyWallet = softCurrencyWallet;
            _caseSpinConfig = caseSpinConfig;
            _count = caseSpinConfig.MinCount;
        }

        protected override void StartPresentingInternal()
        {
            Panel.SpinClicked += SpinClickHandle;
            Panel.BackClicked += BackClickHandle;
            Panel.CountSelected += CountSelectHandle;
            Panel.DropSellClicked += DropSellClickHandle;
            Panel.SellAllClicked += SellAllClickHandle;
            _softCurrencyWallet.Changed += WalletChangeHandle;

            _isPresenting = true;
            _isSpinning = false;

            Panel.SetCaseView(_caseConfig.Name);
            Panel.SetCaseContents(CreateCaseContents());
            Panel.SetCountOptions(_caseSpinConfig.MinCount, _caseSpinConfig.MaxCount);
            Panel.SetSpinState(false);

            ApplyCount(_count);
        }

        public void SetPayload(CaseId payload)
        {
            _caseConfig = _casesDatabaseService.GetCaseById(payload);
        }

        private IReadOnlyList<CaseContentItemView.Data> CreateCaseContents()
        {
            var skinConfigs = _caseConfig.SkinsMapEntries.Entries
                .Select(entry => _skinsDatabaseService.GetSkinConfigById(entry.Key.Id))
                .Distinct()
                .OrderByDescending(skinConfig => skinConfig.Rarity)
                .ThenBy(skinConfig => skinConfig.Name);

            var items = new List<CaseContentItemView.Data>();

            foreach (var skinConfig in skinConfigs)
            {
                var rarityVisual = _skinsUIContextDataService.GetRarityVisual(skinConfig.Rarity);

                items.Add(new CaseContentItemView.Data()
                {
                    Name = skinConfig.Name,
                    Icon = (Sprite)skinConfig.ContextData.SpriteSource.Asset,
                    RarityColor = rarityVisual.RarityMainColor,
                });
            }

            return items;
        }

        private void ApplyCount(int count)
        {
            _count = Mathf.Clamp(count, _caseSpinConfig.MinCount, _caseSpinConfig.MaxCount);

            Panel.SetSelectedCount(_count);
            Panel.SetSpinPrice(GetTotalPrice());
            Panel.PrepareSlotMachines(_count);
            ClearDrops();

            RefreshAffordability();
        }

        private void RefreshAffordability()
        {
            Panel.SetSpinAffordable(CanSpinCase());
        }

        private void CountSelectHandle(int count)
        {
            if (_isSpinning == true || count == _count)
            {
                return;
            }

            ApplyCount(count);
        }

        private void WalletChangeHandle(IWallet sender, float delta, float current)
        {
            RefreshAffordability();
        }

        private void BackClickHandle()
        {
            _isSpinning = false;
            Panel.StopSlotMachines();
            ClearDrops();

            _panelsService.Close(Panel.GetType());
        }

        private void SpinClickHandle()
        {
            if (_isSpinning == true || CanSpinCase() == false)
            {
                return;
            }

            var count = _count;
            var winSlots = new List<SlotData>(count);
            var skins = new List<Skin>(count);
            var rewards = new List<DropResultCard.Data>(count);

            for (var index = 0; index < count; index++)
            {
                var winSlot = DefineWinSlotData();
                var skin = CreateSkin(winSlot);

                winSlots.Add(winSlot);
                skins.Add(skin);
                rewards.Add(CreateRewardData(skin));
            }

            _isSpinning = true;
            Panel.SetSpinState(true);

            _softCurrencyWallet.Spend(GetTotalPrice());

            foreach (var skin in skins)
            {
                _inventoryService.Add(skin);
            }

            var slotMachines = Panel.PrepareSlotMachines(count);
            Panel.SetRewards(rewards);

            _dropsResult.Start(skins);
            RefreshDropsTotal();

            _spinRunId++;
            RunSpins(slotMachines, winSlots, _spinRunId).Forget();
        }

        private async UniTaskVoid RunSpins(IReadOnlyList<HorizontalSlotMachine> slotMachines, IReadOnlyList<SlotData> winSlots, int spinRunId)
        {
            var isMultiple = slotMachines.Count > 1;
            var spins = new List<UniTask>(slotMachines.Count);

            try
            {
                for (var index = 0; index < slotMachines.Count; index++)
                {
                    var extraData = new HorizontalSlotMachine.ExtraData(index * _caseSpinConfig.SpinStagger, isMultiple);

                    spins.Add(slotMachines[index].Spin(winSlots[index], DefineSecondarySlotData, CreateSlot, extraData));
                }

                await UniTask.WhenAll(spins);
            }
            catch (Exception exception) when (exception is OperationCanceledException == false)
            {
                Debug.LogException(exception);
            }
            finally
            {
                if (_isPresenting == true && spinRunId == _spinRunId)
                {
                    _isSpinning = false;
                    Panel.SetSpinState(false);
                    RefreshAffordability();
                }
            }
        }

        private float GetTotalPrice()
        {
            return _caseConfig.Price * _count;
        }

        private void DropSellClickHandle(int index)
        {
            var amount = _skinSellService.Sell(_dropsResult.GetSkin(index));

            _dropsResult.MarkSold(index, amount);
            ShowDropSold(index, amount);

            if (amount > 0f)
            {
                Panel.PlayDropSoldFeedback(index);
            }

            RefreshDropsTotal();
        }

        private void SellAllClickHandle()
        {
            var unsoldIndices = _dropsResult.GetUnsoldIndices();
            var unsoldSkins = _dropsResult.GetUnsoldSkins();
            var amounts = new List<float>(unsoldSkins.Count);

            foreach (var skin in unsoldSkins)
            {
                amounts.Add(_skinSellService.CanSell(skin) == true ? _skinSellService.GetSellPrice(skin) : 0f);
            }

            var amount = _skinSellService.SellAll(unsoldSkins);

            for (var index = 0; index < unsoldIndices.Count; index++)
            {
                _dropsResult.MarkSold(unsoldIndices[index], amounts[index]);
                ShowDropSold(unsoldIndices[index], amounts[index]);
            }

            if (amount > 0f)
            {
                Panel.PlaySellAllFeedback();
            }

            RefreshDropsTotal();
        }

        private void ShowDropSold(int index, float amount)
        {
            if (amount > 0f)
            {
                Panel.MarkDropSold(index, amount);
            }
            else
            {
                Panel.MarkDropUnavailable(index);
            }
        }

        private void RefreshDropsTotal()
        {
            var unsoldSkins = _dropsResult.GetUnsoldSkins();

            Panel.SetDropsTotal(unsoldSkins.Count, _skinSellService.GetSellPrice(unsoldSkins), _dropsResult.SoldTotal);
        }

        private void ClearDrops()
        {
            _dropsResult.Clear();
        }

        private DropResultCard.Data CreateRewardData(Skin skin)
        {
            var skinConfig = _skinsDatabaseService.GetSkinConfigById(skin.ConfigId);
            var rarityVisual = _skinsUIContextDataService.GetRarityVisual(skinConfig.Rarity);
            var wear = _skinsDatabaseService.GetSkinWear(skin);

            return new DropResultCard.Data()
            {
                WeaponName = skinConfig.WeaponName,
                SkinName = skinConfig.Name,
                WearName = _skinsUIContextDataService.GetWearName(wear),
                IsStatTrak = skin.IsStatTrak,
                IsSouvenir = skin.IsSouvenir,
                SellPrice = _skinSellService.GetSellPrice(skin),
                RarityColor = rarityVisual.RarityMainColor,
            };
        }

        private bool CanSpinCase()
        {
            return _softCurrencyWallet.IsEnoughForSpend(GetTotalPrice());
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

            var skinSprite = (Sprite)skinConfig.ContextData.SpriteSource.Asset;
            var rarityVisual = _skinsUIContextDataService.GetRarityVisual(skinConfig.Rarity);

            slot.SetData(skinSprite, rarityVisual);

            return slot;
        }

        protected override void StopPresentingInternal()
        {
            Panel.SpinClicked -= SpinClickHandle;
            Panel.BackClicked -= BackClickHandle;
            Panel.CountSelected -= CountSelectHandle;
            Panel.DropSellClicked -= DropSellClickHandle;
            Panel.SellAllClicked -= SellAllClickHandle;
            _softCurrencyWallet.Changed -= WalletChangeHandle;

            _isPresenting = false;
            _isSpinning = false;

            Panel.StopSlotMachines();
            ClearDrops();
        }
    }
}

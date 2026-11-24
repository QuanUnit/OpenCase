using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Inventory.Services;
using Features.Inventory.UI.Panels;
using Features.Skins.Data;
using Features.Skins.Services;
using Features.Upgrade.Configs;
using Features.Upgrade.Data;
using Features.Upgrade.Services;
using Features.Upgrade.UI.Panels;
using QCore.UISystem.Presenters;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Features.Upgrade.UI.Presenters
{
    public class UpgradeSubPanelPresenter : PanelPresenter<UpgradeSubPanel>
    {
        private const int NoSelection = -1;

        private readonly IUpgradeService _upgradeService;
        private readonly IInventoryService _inventoryService;
        private readonly ISkinsDatabaseService _skinsDatabaseService;
        private readonly ISkinsUIContextDataService _skinsUIContextDataService;
        private readonly UpgradeConfig _upgradeConfig;

        private Skin _source;
        private UpgradeOffer[] _offers;
        private int _selectedIndex = NoSelection;
        private bool _isRolling;
        private CancellationTokenSource _rollCancellation;

        public UpgradeSubPanelPresenter(IUpgradeService upgradeService, IInventoryService inventoryService, ISkinsDatabaseService skinsDatabaseService, ISkinsUIContextDataService skinsUIContextDataService, UpgradeConfig upgradeConfig)
        {
            _upgradeService = upgradeService;
            _inventoryService = inventoryService;
            _skinsDatabaseService = skinsDatabaseService;
            _skinsUIContextDataService = skinsUIContextDataService;
            _upgradeConfig = upgradeConfig;
        }

        protected override void StartPresentingInternal()
        {
            _offers = new UpgradeOffer[_upgradeService.Multipliers.Count];
            _rollCancellation = new CancellationTokenSource();

            Panel.SkinClicked += SkinClickHandle;
            Panel.MultiplierClicked += MultiplierClickHandle;
            Panel.UpgradeClicked += UpgradeClickHandle;

            _inventoryService.Added += SkinAddHandle;
            _inventoryService.Removed += SkinRemoveHandle;

            Panel.SetMultipliers(_upgradeService.Multipliers);
            Panel.AddSkinViews(_inventoryService.AllSkins, CreateSkinView);
            Panel.SetInputBlocked(false);
            Panel.ResetResult();

            ClearSelection();
        }

        private void SkinClickHandle(SkinInstanceId skinId)
        {
            _source = _inventoryService.Get(skinId);
            _selectedIndex = NoSelection;

            for (var index = 0; index < _offers.Length; index++)
            {
                _upgradeService.TryCreateOffer(_source, _upgradeService.Multipliers[index], out var offer);
                _offers[index] = offer;

                if (_selectedIndex == NoSelection && offer != null)
                {
                    _selectedIndex = index;
                }
            }

            Panel.ResetResult();
            Panel.SetSelectedSkin(_source.Id);
            Panel.SetSource(CreateCardData(_source));

            RefreshOffer();
        }

        private void MultiplierClickHandle(int index)
        {
            if (_offers[index] == null)
            {
                return;
            }

            if (index == _selectedIndex)
            {
                var isCreated = _upgradeService.TryCreateOffer(_source, _upgradeService.Multipliers[index], out var offer);

                if (isCreated == true)
                {
                    _offers[index] = offer;
                }
            }

            _selectedIndex = index;

            RefreshOffer();
        }

        private void UpgradeClickHandle()
        {
            if (_selectedIndex == NoSelection || _isRolling == true)
            {
                return;
            }

            Roll(_offers[_selectedIndex], _rollCancellation.Token).Forget();
        }

        private async UniTaskVoid Roll(UpgradeOffer offer, CancellationToken cancellationToken)
        {
            _isRolling = true;
            Panel.SetInputBlocked(true);

            var isSuccess = _upgradeService.Upgrade(offer);

            var isCanceled = await Panel
                .PlayRoll(offer.Chance, isSuccess, _upgradeConfig.RollTurns, _upgradeConfig.RollDuration, cancellationToken)
                .SuppressCancellationThrow();

            if (isCanceled == true || Panel.gameObject.activeInHierarchy == false)
            {
                return;
            }

            Panel.PlayResult(isSuccess, GetRarityColor(offer.Target));

            isCanceled = await UniTask
                .Delay(TimeSpan.FromSeconds(_upgradeConfig.ResultHoldDuration), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, cancellationToken)
                .SuppressCancellationThrow();

            if (isCanceled == true)
            {
                return;
            }

            _isRolling = false;

            RebuildSkinViews();
            ClearSelection();
            Panel.ResetResult();
            Panel.SetInputBlocked(false);
        }

        private void RefreshOffer()
        {
            var availability = new bool[_offers.Length];

            for (var index = 0; index < _offers.Length; index++)
            {
                availability[index] = _offers[index] != null;
            }

            Panel.SetMultiplierStates(availability, _selectedIndex);

            if (_selectedIndex == NoSelection)
            {
                Panel.SetTargetEmpty();
                Panel.SetUpgradeInteractable(false);
                return;
            }

            var selectedOffer = _offers[_selectedIndex];

            Panel.SetTarget(CreateCardData(selectedOffer.Target), selectedOffer.Chance);
            Panel.SetUpgradeInteractable(true);
        }

        private void ClearSelection()
        {
            _source = null;
            _selectedIndex = NoSelection;
            Array.Clear(_offers, 0, _offers.Length);

            Panel.HideSelectionFrame();
            Panel.SetSourceEmpty();

            RefreshOffer();
        }

        private void RebuildSkinViews()
        {
            Panel.HideSelectionFrame();
            Panel.RemoveAllSkinViews();
            Panel.AddSkinViews(_inventoryService.AllSkins, CreateSkinView);
        }

        private void SkinAddHandle(Skin skin)
        {
            if (_isRolling == true)
            {
                return;
            }

            Panel.AddSkinView(skin, CreateSkinView);
        }

        private void SkinRemoveHandle(Skin skin)
        {
            if (_isRolling == true)
            {
                return;
            }

            Panel.RemoveSkinView(skin.Id);

            if (_source != null && _source.Id == skin.Id)
            {
                ClearSelection();
            }
        }

        private UpgradeSkinCard.Data CreateCardData(Skin skin)
        {
            var skinConfig = _skinsDatabaseService.GetSkinConfigById(skin.ConfigId);

            return new UpgradeSkinCard.Data()
            {
                Icon = skinConfig.ContextData.SpriteSource.GetAsset(),
                Name = skinConfig.Name,
                Price = skin.Price,
                RarityColor = GetRarityColor(skin),
            };
        }

        private Color GetRarityColor(Skin skin)
        {
            var rarity = _skinsDatabaseService.GetSkinRarity(skin);

            return _skinsUIContextDataService.GetRarityVisual(rarity).RarityMainColor;
        }

        private SkinView CreateSkinView(Skin skin, SkinView prefab, Transform parent)
        {
            var view = Object.Instantiate(prefab, parent);
            var skinConfig = _skinsDatabaseService.GetSkinConfigById(skin.ConfigId);

            view.SetView(new SkinView.Data()
            {
                Icon = skinConfig.ContextData.SpriteSource.GetAsset(),
                Name = skinConfig.Name,
                Price = skin.Price,
                RarityColor = GetRarityColor(skin),
            });

            return view;
        }

        protected override void StopPresentingInternal()
        {
            _rollCancellation.Cancel();
            _rollCancellation.Dispose();
            _isRolling = false;

            Panel.SkinClicked -= SkinClickHandle;
            Panel.MultiplierClicked -= MultiplierClickHandle;
            Panel.UpgradeClicked -= UpgradeClickHandle;

            _inventoryService.Added -= SkinAddHandle;
            _inventoryService.Removed -= SkinRemoveHandle;

            Panel.HideSelectionFrame();
            Panel.RemoveAllSkinViews();
            Panel.SetInputBlocked(false);
        }
    }
}

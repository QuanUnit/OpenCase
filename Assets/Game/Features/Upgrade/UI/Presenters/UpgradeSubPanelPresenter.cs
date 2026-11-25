using System;
using System.Collections.Generic;
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
using Features.Wallet.Services;
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
        private readonly SoftCurrencyWallet _wallet;
        private readonly UpgradeConfig _upgradeConfig;
        private readonly List<Skin> _targets = new List<Skin>();
        private readonly List<float> _chances = new List<float>();
        private readonly List<UpgradeTargetOption.Data> _targetsData = new List<UpgradeTargetOption.Data>();

        private Skin _source;
        private bool[] _multipliersAvailability;
        private int _multiplierIndex = NoSelection;
        private int _targetIndex = NoSelection;
        private float _extra;
        private bool _isRolling;
        private CancellationTokenSource _rollCancellation;

        public UpgradeSubPanelPresenter(IUpgradeService upgradeService, IInventoryService inventoryService, ISkinsDatabaseService skinsDatabaseService, ISkinsUIContextDataService skinsUIContextDataService, SoftCurrencyWallet wallet, UpgradeConfig upgradeConfig)
        {
            _upgradeService = upgradeService;
            _inventoryService = inventoryService;
            _skinsDatabaseService = skinsDatabaseService;
            _skinsUIContextDataService = skinsUIContextDataService;
            _wallet = wallet;
            _upgradeConfig = upgradeConfig;
        }

        protected override void StartPresentingInternal()
        {
            _multipliersAvailability = new bool[_upgradeService.Multipliers.Count];
            _rollCancellation = new CancellationTokenSource();

            Panel.SkinClicked += SkinClickHandle;
            Panel.MultiplierClicked += MultiplierClickHandle;
            Panel.UpgradeClicked += UpgradeClickHandle;
            Panel.TargetClicked += TargetClickHandle;
            Panel.RefreshTargetsClicked += RefreshTargetsClickHandle;
            Panel.ExtraChanged += ExtraChangeHandle;

            _inventoryService.Added += SkinAddHandle;
            _inventoryService.Removed += SkinRemoveHandle;
            _wallet.Changed += WalletChangeHandle;

            Panel.SetMultipliers(_upgradeService.Multipliers);
            Panel.AddSkinViews(_inventoryService.AllSkins, CreateSkinView);
            Panel.SetInputBlocked(false);
            Panel.ResetResult();

            ClearSelection();
        }

        private void SkinClickHandle(SkinInstanceId skinId)
        {
            _source = _inventoryService.Get(skinId);
            _multiplierIndex = NoSelection;
            _targets.Clear();

            for (var index = 0; index < _multipliersAvailability.Length; index++)
            {
                var targets = _upgradeService.CreateTargets(_source, _upgradeService.Multipliers[index]);
                _multipliersAvailability[index] = targets.Count > 0;

                if (_multiplierIndex == NoSelection && targets.Count > 0)
                {
                    _multiplierIndex = index;
                    _targets.AddRange(targets);
                }
            }

            Panel.ResetResult();
            Panel.SetSelectedSkin(_source.Id);
            Panel.SetSource(CreateCardData(_source));
            Panel.SetMultiplierStates(_multipliersAvailability, _multiplierIndex);

            ShowTargets();
        }

        private void MultiplierClickHandle(int index)
        {
            if (_multipliersAvailability[index] == false)
            {
                return;
            }

            _multiplierIndex = index;

            Panel.SetMultiplierStates(_multipliersAvailability, _multiplierIndex);

            RebuildTargets();
        }

        private void RefreshTargetsClickHandle()
        {
            if (_multiplierIndex == NoSelection)
            {
                return;
            }

            RebuildTargets();
        }

        private void TargetClickHandle(int index)
        {
            _targetIndex = index;

            Panel.SetSelectedTarget(_targetIndex);

            RefreshExtraAndChances();
        }

        private void ExtraChangeHandle(float value)
        {
            _extra = value;

            RefreshChances();
        }

        private void WalletChangeHandle(IWallet sender, float delta, float current)
        {
            if (_isRolling == true || _targetIndex == NoSelection)
            {
                return;
            }

            RefreshExtraAndChances();
        }

        private void UpgradeClickHandle()
        {
            if (_targetIndex == NoSelection || _isRolling == true)
            {
                return;
            }

            _isRolling = true;

            var offer = _upgradeService.CreateOffer(_source, _targets[_targetIndex], _extra);
            var result = _upgradeService.Upgrade(offer);

            if (result == UpgradeResult.Rejected)
            {
                _isRolling = false;

                RefreshExtraAndChances();
                return;
            }

            Roll(offer, result == UpgradeResult.Success, _rollCancellation.Token).Forget();
        }

        private async UniTaskVoid Roll(UpgradeOffer offer, bool isSuccess, CancellationToken cancellationToken)
        {
            Panel.SetInputBlocked(true);

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

        private void RebuildTargets()
        {
            _targets.Clear();
            _targets.AddRange(_upgradeService.CreateTargets(_source, _upgradeService.Multipliers[_multiplierIndex]));

            if (_targets.Count == 0)
            {
                _multipliersAvailability[_multiplierIndex] = false;

                Panel.SetMultiplierStates(_multipliersAvailability, _multiplierIndex);
            }

            ShowTargets();
        }

        private void ShowTargets()
        {
            Panel.SetRefreshInteractable(_multiplierIndex != NoSelection);

            if (_targets.Count == 0)
            {
                _targetIndex = NoSelection;
                _extra = 0f;

                Panel.SetTargetsEmpty();
                Panel.SetTargetEmpty();
                Panel.SetExtraRange(0f);
                Panel.SetExtra(_extra);
                Panel.SetUpgradeInteractable(false);
                return;
            }

            _targetIndex = 0;
            _targetsData.Clear();

            foreach (var target in _targets)
            {
                _targetsData.Add(CreateTargetData(target));
            }

            Panel.SetTargets(_targetsData);
            Panel.SetSelectedTarget(_targetIndex);
            Panel.SetUpgradeInteractable(true);

            RefreshExtraAndChances();
        }

        private void RefreshExtraAndChances()
        {
            var maxExtra = _upgradeService.GetMaxExtra(_source, _targets[_targetIndex]);
            _extra = Mathf.Clamp(_extra, 0f, maxExtra);

            Panel.SetExtraRange(maxExtra);
            Panel.SetExtra(_extra);

            RefreshChances();
        }

        private void RefreshChances()
        {
            _chances.Clear();

            foreach (var target in _targets)
            {
                _chances.Add(_upgradeService.GetChance(_source, target, _extra));
            }

            Panel.SetTargetChances(_chances);
            Panel.SetTarget(CreateCardData(_targets[_targetIndex]), _chances[_targetIndex]);
        }

        private void ClearSelection()
        {
            _source = null;
            _multiplierIndex = NoSelection;
            _targets.Clear();
            Array.Clear(_multipliersAvailability, 0, _multipliersAvailability.Length);

            Panel.HideSelectionFrame();
            Panel.SetSourceEmpty();
            Panel.SetMultiplierStates(_multipliersAvailability, _multiplierIndex);

            ShowTargets();
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
                Icon = (Sprite)skinConfig.ContextData.SpriteSource.Asset,
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

        private UpgradeTargetOption.Data CreateTargetData(Skin skin)
        {
            var skinConfig = _skinsDatabaseService.GetSkinConfigById(skin.ConfigId);

            return new UpgradeTargetOption.Data()
            {
                Icon = (Sprite)skinConfig.ContextData.SpriteSource.Asset,
                Name = skinConfig.Name,
                Price = skin.Price,
                RarityColor = GetRarityColor(skin),
                Chance = _upgradeService.GetChance(_source, skin, _extra),
            };
        }

        private SkinView CreateSkinView(Skin skin, SkinView prefab, Transform parent)
        {
            var view = Object.Instantiate(prefab, parent);
            var skinConfig = _skinsDatabaseService.GetSkinConfigById(skin.ConfigId);

            view.SetView(new SkinView.Data()
            {
                Icon = (Sprite)skinConfig.ContextData.SpriteSource.Asset,
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
            Panel.TargetClicked -= TargetClickHandle;
            Panel.RefreshTargetsClicked -= RefreshTargetsClickHandle;
            Panel.ExtraChanged -= ExtraChangeHandle;

            _inventoryService.Added -= SkinAddHandle;
            _inventoryService.Removed -= SkinRemoveHandle;
            _wallet.Changed -= WalletChangeHandle;

            Panel.HideSelectionFrame();
            Panel.RemoveAllSkinViews();
            Panel.SetInputBlocked(false);
        }
    }
}

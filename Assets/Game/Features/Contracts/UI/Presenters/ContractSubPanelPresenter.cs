using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Contracts.Configs;
using Features.Contracts.Services;
using Features.Contracts.UI.Panels;
using Features.Inventory.Services;
using Features.Inventory.UI.Panels;
using Features.Skins.Configs;
using Features.Skins.Data;
using Features.Skins.Services;
using QCore.Tools.Identifiers;
using QCore.UISystem.Presenters;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Features.Contracts.UI.Presenters
{
    public class ContractSubPanelPresenter : PanelPresenter<ContractSubPanel>
    {
        private readonly IContractService _contractService;
        private readonly IInventoryService _inventoryService;
        private readonly ISkinsDatabaseService _skinsDatabaseService;
        private readonly ISkinsUIContextDataService _skinsUIContextDataService;
        private readonly ContractConfig _contractConfig;

        private readonly List<Skin> _inputs = new List<Skin>();
        private bool _isExecuting;
        private CancellationTokenSource _executeCancellation;

        public ContractSubPanelPresenter(IContractService contractService, IInventoryService inventoryService, ISkinsDatabaseService skinsDatabaseService, ISkinsUIContextDataService skinsUIContextDataService, ContractConfig contractConfig)
        {
            _contractService = contractService;
            _inventoryService = inventoryService;
            _skinsDatabaseService = skinsDatabaseService;
            _skinsUIContextDataService = skinsUIContextDataService;
            _contractConfig = contractConfig;
        }

        protected override void StartPresentingInternal()
        {
            _executeCancellation = new CancellationTokenSource();

            Panel.SkinClicked += SkinClickHandle;
            Panel.SlotClicked += SlotClickHandle;
            Panel.SignClicked += SignClickHandle;
            Panel.SignatureClearClicked += SignatureClearClickHandle;
            Panel.SignatureConfirmClicked += SignatureConfirmClickHandle;
            Panel.SignatureCancelClicked += SignatureCancelClickHandle;
            Panel.SignatureCoverageChanged += SignatureCoverageChangeHandle;
            Panel.ResultCloseClicked += ResultCloseClickHandle;

            _inventoryService.Added += SkinAddHandle;
            _inventoryService.Removed += SkinRemoveHandle;

            Panel.HideSignature();
            Panel.HideResult();
            Panel.SetInputBlocked(false);
            Panel.AddSkinViews(_inventoryService.AllSkins, CreateSkinView);

            Refresh();
        }

        private void SkinClickHandle(QGuid skinId)
        {
            var skin = _inventoryService.Get(skinId);

            if (_contractService.CanAdd(_inputs, skin) == false)
            {
                return;
            }

            _inputs.Add(skin);

            Refresh();
        }

        private void SlotClickHandle(int index)
        {
            if (index >= _inputs.Count)
            {
                return;
            }

            _inputs.RemoveAt(index);

            Refresh();
        }

        private void SignClickHandle()
        {
            if (_inputs.Count != _contractService.InputsCount)
            {
                return;
            }

            Panel.ShowSignature();
        }

        private void SignatureClearClickHandle()
        {
            Panel.ClearSignature();
            Panel.SetSignatureConfirmInteractable(false);
        }

        private void SignatureCoverageChangeHandle(float coverage)
        {
            if (_isExecuting == true)
            {
                return;
            }

            Panel.SetSignatureConfirmInteractable(coverage >= _contractConfig.MinInkCoverage);
        }

        private void SignatureCancelClickHandle()
        {
            Panel.HideSignature();
        }

        private void SignatureConfirmClickHandle()
        {
            if (_isExecuting == true)
            {
                return;
            }

            Execute(_executeCancellation.Token).Forget();
        }

        private async UniTaskVoid Execute(CancellationToken cancellationToken)
        {
            _isExecuting = true;
            Panel.SetSignatureButtonsInteractable(false);

            var isExecuted = _contractService.TryExecute(_inputs, out var result);

            if (isExecuted == false)
            {
                _isExecuting = false;
                Panel.HideSignature();
                RemoveMissingInputs();
                Refresh();
                return;
            }

            _inputs.Clear();

            var isCanceled = await UniTask
                .Delay(TimeSpan.FromSeconds(_contractConfig.ResultRevealDelay), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, cancellationToken)
                .SuppressCancellationThrow();

            if (isCanceled == true || Panel.gameObject.activeInHierarchy == false)
            {
                return;
            }

            var config = _skinsDatabaseService.GetSkinConfigById(result.SkinSourceId);

            Panel.HideSignature();
            Panel.SetInputBlocked(true);
            Panel.ShowResult(new ContractResultView.Data()
            {
                Icon = config.ContextData.SpriteSource.GetAsset(),
                Name = config.Name,
                Price = result.Price,
                RarityColor = GetRarityColor(config.Rarity),
            });

            Refresh();
        }

        private void ResultCloseClickHandle()
        {
            _isExecuting = false;

            Panel.HideResult();
            Panel.SetInputBlocked(false);
        }

        private void Refresh()
        {
            foreach (var skinId in Panel.SkinIds.ToList())
            {
                var skin = _inventoryService.Get(skinId);
                var isInContract = _inputs.Any(input => input.Id == skinId);
                var isAvailable = isInContract == false && _contractService.CanAdd(_inputs, skin);

                Panel.SetSkinState(skinId, isInContract, isAvailable);
            }

            for (var index = 0; index < Panel.SlotsCount; index++)
            {
                if (index < _inputs.Count)
                {
                    var config = _skinsDatabaseService.GetSkinConfigById(_inputs[index].SkinSourceId);
                    Panel.SetSlot(index, config.ContextData.SpriteSource.GetAsset(), GetRarityColor(config.Rarity));
                    continue;
                }

                Panel.SetSlotEmpty(index);
            }

            var outcomes = _contractService
                .GetOutcomes(_inputs)
                .Select(outcome => new ContractOutcomeView.Data()
                {
                    Icon = outcome.Config.ContextData.SpriteSource.GetAsset(),
                    Name = outcome.Config.Name,
                    Chance = outcome.Chance,
                    RarityColor = GetRarityColor(outcome.Config.Rarity),
                })
                .ToList();

            Panel.SetOutcomes(outcomes);
            Panel.SetSummary(_inputs.Count, _contractService.InputsCount, _inputs.Sum(input => input.Price));
            Panel.SetSignInteractable(_inputs.Count == _contractService.InputsCount);
            Panel.SetEmptyState(_inventoryService.AllSkins.Any() == false);
        }

        private void RemoveMissingInputs()
        {
            _inputs.RemoveAll(input => _inventoryService.TryGet(input.Id, out _) == false);
        }

        private void SkinAddHandle(Skin skin)
        {
            Panel.AddSkinView(skin, CreateSkinView);

            if (_isExecuting == false)
            {
                Refresh();
            }
        }

        private void SkinRemoveHandle(Skin skin)
        {
            Panel.RemoveSkinView(skin.Id);

            if (_isExecuting == true)
            {
                return;
            }

            _inputs.RemoveAll(input => input.Id == skin.Id);

            Refresh();
        }

        private Color GetRarityColor(Rarity rarity)
        {
            return _skinsUIContextDataService.GetRarityVisual(rarity).RarityMainColor;
        }

        private SkinView CreateSkinView(Skin skin, SkinView prefab, Transform parent)
        {
            var view = Object.Instantiate(prefab, parent);
            var skinConfig = _skinsDatabaseService.GetSkinConfigById(skin.SkinSourceId);

            view.SetView(new SkinView.Data()
            {
                Icon = skinConfig.ContextData.SpriteSource.GetAsset(),
                Name = skinConfig.Name,
                Price = skin.Price,
                RarityColor = GetRarityColor(skinConfig.Rarity),
            });

            return view;
        }

        protected override void StopPresentingInternal()
        {
            _executeCancellation.Cancel();
            _executeCancellation.Dispose();
            _isExecuting = false;

            Panel.SkinClicked -= SkinClickHandle;
            Panel.SlotClicked -= SlotClickHandle;
            Panel.SignClicked -= SignClickHandle;
            Panel.SignatureClearClicked -= SignatureClearClickHandle;
            Panel.SignatureConfirmClicked -= SignatureConfirmClickHandle;
            Panel.SignatureCancelClicked -= SignatureCancelClickHandle;
            Panel.SignatureCoverageChanged -= SignatureCoverageChangeHandle;
            Panel.ResultCloseClicked -= ResultCloseClickHandle;

            _inventoryService.Added -= SkinAddHandle;
            _inventoryService.Removed -= SkinRemoveHandle;

            Panel.RemoveAllSkinViews();
            Panel.HideSignature();
            Panel.HideResult();
            Panel.SetInputBlocked(false);
        }
    }
}

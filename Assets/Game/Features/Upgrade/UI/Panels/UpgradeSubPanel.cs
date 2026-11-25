using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Inventory.UI.Panels;
using Features.Skins.Data;
using QCore.UISystem.Panels;
using Shared.UI.Effects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Upgrade.UI.Panels
{
    public class UpgradeSubPanel : Panel
    {
        public event Action<SkinInstanceId> SkinClicked;
        public event Action<int> MultiplierClicked;
        public event Action UpgradeClicked;
        public event Action<int> TargetClicked;
        public event Action RefreshTargetsClicked;
        public event Action<float> ExtraChanged;

        [Header("Inventory")]
        [SerializeField] private Transform _skinsViewport;
        [SerializeField] private SkinView _skinViewPrefab;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private RectTransform _selectionFrame;

        [Header("Upgrade")]
        [SerializeField] private UpgradeSkinCard _sourceCard;
        [SerializeField] private UpgradeSkinCard _targetCard;
        [SerializeField] private UpgradeChanceDial _chanceDial;
        [SerializeField] private Transform _multipliersRoot;
        [SerializeField] private UpgradeMultiplierOption _multiplierOptionPrefab;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private CanvasGroup _inputGroup;

        [Header("Targets")]
        [SerializeField] private ScrollRect _targetsScroll;
        [SerializeField] private Transform _targetsRoot;
        [SerializeField] private UpgradeTargetOption _targetOptionPrefab;
        [SerializeField] private GameObject _targetsEmptyState;
        [SerializeField] private Button _refreshTargetsButton;

        [Header("Extra")]
        [SerializeField] private UpgradeExtraMoneyInput _extraInput;

        [Header("Result")]
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private string _successText = "UPGRADED";
        [SerializeField] private string _failText = "LOST";
        [SerializeField] private Color _successColor;
        [SerializeField] private Color _failColor;
        [SerializeField] private ScreenFlash _screenFlash;

        private readonly Dictionary<SkinInstanceId, SkinView> _activeViews = new Dictionary<SkinInstanceId, SkinView>();
        private readonly Dictionary<SkinView, SkinInstanceId> _viewToSkinIdMap = new Dictionary<SkinView, SkinInstanceId>();
        private readonly List<UpgradeMultiplierOption> _multiplierOptions = new List<UpgradeMultiplierOption>();
        private readonly List<UpgradeTargetOption> _targetOptions = new List<UpgradeTargetOption>();

        private void OnEnable()
        {
            _upgradeButton.onClick.AddListener(UpgradeButtonClickHandle);
            _refreshTargetsButton.onClick.AddListener(RefreshTargetsButtonClickHandle);
            _extraInput.ValueChanged += ExtraValueChangeHandle;
            _resultText.gameObject.SetActive(false);
        }

        public void AddSkinViews(IEnumerable<Skin> skins, InventorySubPanel.CreateSkinViewAction createAction)
        {
            foreach (var skin in skins)
            {
                AddSkinView(skin, createAction);
            }
        }

        public void AddSkinView(Skin skin, InventorySubPanel.CreateSkinViewAction createAction)
        {
            var created = createAction.Invoke(skin, _skinViewPrefab, _skinsViewport);

            created.Clicked += SkinViewClickHandle;
            _activeViews.Add(skin.Id, created);
            _viewToSkinIdMap.Add(created, skin.Id);

            UpdateEmptyState();
        }

        public void RemoveSkinView(SkinInstanceId id)
        {
            var removed = _activeViews.Remove(id, out var view);

            if (removed == false)
            {
                return;
            }

            _viewToSkinIdMap.Remove(view);

            if (_selectionFrame.parent == view.transform)
            {
                HideSelectionFrame();
            }

            view.Clicked -= SkinViewClickHandle;
            Destroy(view.gameObject);

            UpdateEmptyState();
        }

        public void RemoveAllSkinViews()
        {
            while (_activeViews.Count > 0)
            {
                RemoveSkinView(_activeViews.First().Key);
            }
        }

        public void SetSelectedSkin(SkinInstanceId skinId)
        {
            var view = _activeViews[skinId];

            _selectionFrame.SetParent(view.transform, false);
            _selectionFrame.SetAsLastSibling();
            _selectionFrame.anchorMin = Vector2.zero;
            _selectionFrame.anchorMax = Vector2.one;
            _selectionFrame.offsetMin = Vector2.zero;
            _selectionFrame.offsetMax = Vector2.zero;
            _selectionFrame.gameObject.SetActive(true);
        }

        public void HideSelectionFrame()
        {
            _selectionFrame.gameObject.SetActive(false);
            _selectionFrame.SetParent(transform, false);
        }

        public void SetMultipliers(IReadOnlyList<float> multipliers)
        {
            RemoveMultiplierOptions();

            foreach (var multiplier in multipliers)
            {
                var option = Instantiate(_multiplierOptionPrefab, _multipliersRoot);
                option.SetMultiplier(multiplier);
                option.Clicked += MultiplierOptionClickHandle;

                _multiplierOptions.Add(option);
            }
        }

        public void SetMultiplierStates(IReadOnlyList<bool> availability, int selectedIndex)
        {
            for (var index = 0; index < _multiplierOptions.Count; index++)
            {
                _multiplierOptions[index].SetState(availability[index], index == selectedIndex);
            }
        }

        public void SetTargets(IReadOnlyList<UpgradeTargetOption.Data> targets)
        {
            RemoveTargetOptions();

            foreach (var target in targets)
            {
                var option = Instantiate(_targetOptionPrefab, _targetsRoot);
                option.SetView(target);
                option.SetSelected(false);
                option.Clicked += TargetOptionClickHandle;

                _targetOptions.Add(option);
            }

            _targetsEmptyState.SetActive(_targetOptions.Count == 0);
            _targetsScroll.verticalNormalizedPosition = 1f;
        }

        public void SetTargetsEmpty()
        {
            RemoveTargetOptions();

            _targetsEmptyState.SetActive(true);
        }

        public void SetSelectedTarget(int index)
        {
            for (var optionIndex = 0; optionIndex < _targetOptions.Count; optionIndex++)
            {
                _targetOptions[optionIndex].SetSelected(optionIndex == index);
            }
        }

        public void SetTargetChances(IReadOnlyList<float> chances)
        {
            for (var index = 0; index < _targetOptions.Count; index++)
            {
                _targetOptions[index].SetChance(chances[index]);
            }
        }

        public void SetRefreshInteractable(bool isInteractable)
        {
            _refreshTargetsButton.interactable = isInteractable;
        }

        public void SetExtraRange(float max)
        {
            _extraInput.SetRange(max);
        }

        public void SetExtra(float value)
        {
            _extraInput.SetValue(value);
        }

        public void SetSource(UpgradeSkinCard.Data data)
        {
            _sourceCard.SetView(data);
        }

        public void SetSourceEmpty()
        {
            _sourceCard.SetEmpty();
        }

        public void SetTarget(UpgradeSkinCard.Data data, float chance)
        {
            _targetCard.SetView(data);
            _chanceDial.SetChance(chance);
        }

        public void SetTargetEmpty()
        {
            _targetCard.SetEmpty();
            _chanceDial.SetEmpty();
        }

        public void SetUpgradeInteractable(bool isInteractable)
        {
            _upgradeButton.interactable = isInteractable;
        }

        public void SetInputBlocked(bool isBlocked)
        {
            _inputGroup.interactable = isBlocked == false;
            _inputGroup.blocksRaycasts = isBlocked == false;
        }

        public void ResetResult()
        {
            _resultText.gameObject.SetActive(false);
            _chanceDial.ResetPointer();
        }

        public UniTask PlayRoll(float chance, bool isSuccess, int turns, float duration, CancellationToken cancellationToken)
        {
            ResetResult();

            return _chanceDial.PlayRoll(chance, isSuccess, turns, duration, cancellationToken);
        }

        public void PlayResult(bool isSuccess, Color rarityColor)
        {
            _resultText.gameObject.SetActive(true);

            if (isSuccess == true)
            {
                _resultText.text = _successText;
                _resultText.color = _successColor;
                _targetCard.PlayWin();
                _screenFlash.Play(rarityColor);
                return;
            }

            _resultText.text = _failText;
            _resultText.color = _failColor;
            _sourceCard.PlayLost();
            _targetCard.PlayLost();
            _screenFlash.Play(_failColor);
        }

        private void UpdateEmptyState()
        {
            _emptyState.SetActive(_activeViews.Count == 0);
        }

        private void RemoveMultiplierOptions()
        {
            foreach (var option in _multiplierOptions)
            {
                option.Clicked -= MultiplierOptionClickHandle;
                Destroy(option.gameObject);
            }

            _multiplierOptions.Clear();
        }

        private void RemoveTargetOptions()
        {
            foreach (var option in _targetOptions)
            {
                option.Clicked -= TargetOptionClickHandle;
                option.gameObject.SetActive(false);
                Destroy(option.gameObject);
            }

            _targetOptions.Clear();
        }

        private void TargetOptionClickHandle(UpgradeTargetOption sender)
        {
            TargetClicked?.Invoke(_targetOptions.IndexOf(sender));
        }

        private void RefreshTargetsButtonClickHandle()
        {
            RefreshTargetsClicked?.Invoke();
        }

        private void ExtraValueChangeHandle(float value)
        {
            ExtraChanged?.Invoke(value);
        }

        private void SkinViewClickHandle(SkinView sender)
        {
            SkinClicked?.Invoke(_viewToSkinIdMap[sender]);
        }

        private void MultiplierOptionClickHandle(UpgradeMultiplierOption sender)
        {
            MultiplierClicked?.Invoke(_multiplierOptions.IndexOf(sender));
        }

        private void UpgradeButtonClickHandle()
        {
            UpgradeClicked?.Invoke();
        }

        private void OnDisable()
        {
            _upgradeButton.onClick.RemoveListener(UpgradeButtonClickHandle);
            _refreshTargetsButton.onClick.RemoveListener(RefreshTargetsButtonClickHandle);
            _extraInput.ValueChanged -= ExtraValueChangeHandle;
        }
    }
}

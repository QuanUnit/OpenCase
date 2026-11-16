using System;
using System.Collections.Generic;
using System.Linq;
using QCore.UISystem.Panels;
using Features.SlotMachine.UI;
using Shared.Tools.Collections;
using Shared.UI;
using Shared.UI.Effects;
using Shared.UI.Layout;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Cases.UI.Panels
{
    public class CaseSpinSubPanel : Panel
    {
        public event Action SpinClicked;
        public event Action BackClicked;
        public event Action<int> CountSelected;

        [Header("Slot machines")]
        [SerializeField] private UniformScaleFitter _slotMachineCellPrefab;
        [SerializeField] private RectTransform _slotMachinesStack;
        [SerializeField, Range(0f, 1f)] private float _multipleSpinsScale = 0.6f;

        [Header("Count selector")]
        [SerializeField] private SpinCountOptionView _countOptionPrefab;
        [SerializeField] private RectTransform _countOptionsContainer;
        [SerializeField] private CanvasGroup _countOptionsCanvasGroup;

        [Header("Spin button")]
        [SerializeField] private Button _spinButton;
        [SerializeField] private CanvasGroup _spinButtonCanvasGroup;
        [SerializeField] private Image _spinButtonVisual;
        [SerializeField] private Graphic[] _spinButtonLabels;
        [SerializeField] private TMP_Text _spinPriceText;
        [SerializeField] private TextedCurrencyView _spinPrice;
        [SerializeField] private GameObject _spinButtonGlow;
        [SerializeField] private Behaviour _spinButtonShine;
        [SerializeField] private Color _affordableFillColor = new Color32(0xFF, 0x8A, 0x1F, 0xFF);
        [SerializeField] private Color _affordableLabelColor = new Color32(0x0B, 0x0D, 0x12, 0xFF);
        [SerializeField] private Color _unaffordableFillColor = new Color32(0x1D, 0x21, 0x2C, 0xFF);
        [SerializeField] private Color _unaffordableLabelColor = new Color32(0x8A, 0x93, 0xA6, 0xFF);
        [SerializeField] private Color _unaffordablePriceColor = new Color32(0xFF, 0x4D, 0x5E, 0xFF);
        [SerializeField, Range(0f, 1f)] private float _spinningAlpha = 0.5f;

        [Header("Header")]
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _caseName;
        [SerializeField] private RectTransform _idleHint;

        [Header("Single reward")]
        [SerializeField] private RectTransform _reward;
        [SerializeField] private TMP_Text _rewardName;
        [SerializeField] private TextedCurrencyView _rewardPrice;
        [SerializeField] private Image _rewardRarity;

        [Header("Rewards summary")]
        [SerializeField] private RectTransform _summary;
        [SerializeField] private TMP_Text _summaryCaption;
        [SerializeField] private string _summaryCaptionFormat = "{0} ITEMS · TOTAL";
        [SerializeField] private TextedCurrencyView _summaryTotal;
        [SerializeField] private TMP_Text _summaryBestName;
        [SerializeField] private string _summaryBestNameFormat = "BEST: {0}";
        [SerializeField] private Image _summaryBestRarity;

        [Header("Case contents")]
        [SerializeField] private CaseContentItemView _caseContentItemPrefab;
        [SerializeField] private RectTransform _caseContentsGrid;
        [SerializeField] private TMP_Text _caseContentsCountText;
        [SerializeField] private string _caseContentsCountFormat = "{0} ITEMS";
        [SerializeField] private ScrollRect _scrollRect;

        [Header("Effects")]
        [SerializeField] private ScreenFlash _screenFlash;

        private readonly List<LayoutElement> _slotMachineCells = new List<LayoutElement>();
        private readonly List<HorizontalSlotMachine> _slotMachines = new List<HorizontalSlotMachine>();
        private readonly List<HorizontalSlotMachine> _activeSlotMachines = new List<HorizontalSlotMachine>();
        private readonly List<CaseContentItemView> _caseContentItems = new List<CaseContentItemView>();
        private readonly List<SpinCountOptionView> _countOptions = new List<SpinCountOptionView>();
        private readonly List<RewardData> _rewards = new List<RewardData>();

        private int _completedSpinsCount;
        private bool _isSpinning;
        private bool _isAffordable = true;

        private void OnEnable()
        {
            _spinButton.onClick.AddListener(SpinClickHandle);
            _backButton.onClick.AddListener(BackClickHandle);

            _rewards.Clear();
            _completedSpinsCount = 0;

            _idleHint.gameObject.SetActive(true);
            _reward.gameObject.SetActive(false);
            _summary.gameObject.SetActive(false);
        }

        public void SetCaseView(string caseName)
        {
            _caseName.text = caseName;
        }

        public void SetSpinPrice(float price)
        {
            _spinPrice.SetView(price);
        }

        public void SetCaseContents(IReadOnlyList<CaseContentItemView.Data> items)
        {
            while (_caseContentItems.Count < items.Count)
            {
                _caseContentItems.Add(Instantiate(_caseContentItemPrefab, _caseContentsGrid));
            }

            for (var index = 0; index < _caseContentItems.Count; index++)
            {
                var item = _caseContentItems[index];
                var isActive = index < items.Count;

                item.gameObject.SetActive(isActive);

                if (isActive == true)
                {
                    item.SetView(items[index]);
                }
            }

            _caseContentsCountText.text = string.Format(_caseContentsCountFormat, items.Count);
            _scrollRect.verticalNormalizedPosition = 1f;
        }

        public void SetCountOptions(int minCount, int maxCount)
        {
            var optionsCount = maxCount - minCount + 1;

            if (_countOptions.Count == optionsCount && _countOptions[0].Count == minCount)
            {
                return;
            }

            foreach (var countOption in _countOptions)
            {
                countOption.Clicked -= CountOptionClickHandle;
                Destroy(countOption.gameObject);
            }

            _countOptions.Clear();

            for (var count = minCount; count <= maxCount; count++)
            {
                var countOption = Instantiate(_countOptionPrefab, _countOptionsContainer);
                countOption.SetCount(count);
                countOption.Clicked += CountOptionClickHandle;

                _countOptions.Add(countOption);
            }

            RefreshCountOptionsInteractable();
        }

        public void SetSelectedCount(int count)
        {
            foreach (var countOption in _countOptions)
            {
                countOption.SetSelected(countOption.Count == count);
            }
        }

        public IReadOnlyList<HorizontalSlotMachine> PrepareSlotMachines(int count)
        {
            while (_slotMachines.Count < count)
            {
                CreateSlotMachine();
            }

            _activeSlotMachines.Clear();

            var scale = count == 1 ? 1f : _multipleSpinsScale;
            var cellHeight = _slotMachineCellPrefab.Target.rect.height * scale;

            for (var index = 0; index < _slotMachines.Count; index++)
            {
                var slotMachine = _slotMachines[index];
                var cell = _slotMachineCells[index];
                var isActive = index < count;

                slotMachine.StopSpinAndClear();

                cell.minHeight = cellHeight;
                cell.preferredHeight = cellHeight;
                cell.gameObject.SetActive(isActive);

                if (isActive == true)
                {
                    _activeSlotMachines.Add(slotMachine);
                }
            }

            _rewards.Clear();
            _completedSpinsCount = 0;

            _idleHint.gameObject.SetActive(true);
            _reward.gameObject.SetActive(false);
            _summary.gameObject.SetActive(false);

            return _activeSlotMachines;
        }

        public void StopSlotMachines()
        {
            foreach (var slotMachine in _slotMachines)
            {
                slotMachine.StopSpinAndClear();
            }

            _rewards.Clear();
            _completedSpinsCount = 0;
        }

        public void SetSpinState(bool isSpinning)
        {
            _isSpinning = isSpinning;

            var alpha = isSpinning == true ? _spinningAlpha : 1f;

            _countOptionsCanvasGroup.alpha = alpha;
            _spinButtonCanvasGroup.alpha = alpha;

            RefreshCountOptionsInteractable();
            RefreshSpinButtonInteractable();
        }

        public void SetSpinAffordable(bool isAffordable)
        {
            _isAffordable = isAffordable;

            var labelColor = isAffordable == true ? _affordableLabelColor : _unaffordableLabelColor;

            _spinButtonVisual.color = isAffordable == true ? _affordableFillColor : _unaffordableFillColor;

            foreach (var label in _spinButtonLabels)
            {
                label.color = labelColor;
            }

            _spinPriceText.color = isAffordable == true ? _affordableLabelColor : _unaffordablePriceColor;
            _spinButtonGlow.SetActive(isAffordable);
            _spinButtonShine.enabled = isAffordable;

            RefreshSpinButtonInteractable();
        }

        public void SetRewards(IReadOnlyList<RewardData> rewards)
        {
            _rewards.Clear();
            _rewards.AddRange(rewards);
            _completedSpinsCount = 0;

            _idleHint.gameObject.SetActive(false);
            _reward.gameObject.SetActive(false);
            _summary.gameObject.SetActive(false);
        }

        private void CreateSlotMachine()
        {
            var cell = Instantiate(_slotMachineCellPrefab, _slotMachinesStack);
            var slotMachine = cell.Target.GetComponent<HorizontalSlotMachine>();

            cell.name = $"{_slotMachineCellPrefab.name} ({_slotMachines.Count})";
            slotMachine.SpinCompleted += SpinCompleteHandle;

            _slotMachineCells.Add(cell.GetComponent<LayoutElement>());
            _slotMachines.Add(slotMachine);
            _idleHint.SetAsLastSibling();
        }

        private void RefreshCountOptionsInteractable()
        {
            foreach (var countOption in _countOptions)
            {
                countOption.SetInteractable(_isSpinning == false);
            }
        }

        private void RefreshSpinButtonInteractable()
        {
            _spinButton.interactable = _isSpinning == false && _isAffordable == true;
        }

        private void SpinCompleteHandle(Slot winSlot)
        {
            _completedSpinsCount++;

            if (_rewards.Count == 0 || _completedSpinsCount < _rewards.Count)
            {
                return;
            }

            if (_rewards.Count == 1)
            {
                ShowSingleReward(_rewards[0]);
            }
            else
            {
                ShowRewardsSummary();
            }
        }

        private void ShowSingleReward(RewardData reward)
        {
            _rewardName.text = reward.SkinName;
            _rewardPrice.SetView(reward.Price);
            _rewardRarity.color = reward.RarityColor;
            _reward.gameObject.SetActive(true);

            _screenFlash.Play(reward.RarityColor);
        }

        private void ShowRewardsSummary()
        {
            var bestReward = _rewards[_rewards.IndexOfMax(reward => reward.Price)];

            _summaryCaption.text = string.Format(_summaryCaptionFormat, _rewards.Count);
            _summaryTotal.SetView(_rewards.Sum(reward => reward.Price));
            _summaryBestName.text = string.Format(_summaryBestNameFormat, bestReward.SkinName);
            _summaryBestRarity.color = bestReward.RarityColor;
            _summary.gameObject.SetActive(true);

            _screenFlash.Play(bestReward.RarityColor);
        }

        private void CountOptionClickHandle(int count)
        {
            CountSelected?.Invoke(count);
        }

        private void BackClickHandle()
        {
            BackClicked?.Invoke();
        }

        private void SpinClickHandle()
        {
            SpinClicked?.Invoke();
        }

        private void OnDisable()
        {
            _spinButton.onClick.RemoveListener(SpinClickHandle);
            _backButton.onClick.RemoveListener(BackClickHandle);
        }

        private void OnDestroy()
        {
            foreach (var slotMachine in _slotMachines)
            {
                slotMachine.SpinCompleted -= SpinCompleteHandle;
            }

            foreach (var countOption in _countOptions)
            {
                countOption.Clicked -= CountOptionClickHandle;
            }
        }

        public struct RewardData
        {
            public string SkinName;
            public Color RarityColor;
            public float Price;
        }
    }
}

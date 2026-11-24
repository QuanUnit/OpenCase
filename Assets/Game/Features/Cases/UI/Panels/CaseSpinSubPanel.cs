using System;
using System.Collections.Generic;
using QCore.UISystem.Panels;
using Features.SlotMachine.UI;
using Shared.Tools.Collections;
using Shared.UI;
using Shared.UI.Effects;
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
        public event Action<int> DropSellClicked;
        public event Action SellAllClicked;

        [Header("Slot machines")]
        [SerializeField] private SlotMachineCellView _slotMachineCellPrefab;
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

        [Header("Drops")]
        [SerializeField] private GameObject _dropsTotalSlot;
        [SerializeField] private DropsTotalBar _dropsTotalBar;
        [SerializeField] private MoneyBurst _moneyBurst;
        [SerializeField, Min(0)] private int _sellParticlesCount = 12;
        [SerializeField, Min(0)] private int _sellAllParticlesCount = 32;

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
        private readonly List<DropResultCard> _dropResultCards = new List<DropResultCard>();
        private readonly List<CaseContentItemView> _caseContentItems = new List<CaseContentItemView>();
        private readonly List<SpinCountOptionView> _countOptions = new List<SpinCountOptionView>();
        private readonly List<DropResultCard.Data> _rewards = new List<DropResultCard.Data>();

        private int _completedSpinsCount;
        private bool _isSpinning;
        private bool _isAffordable = true;

        private void OnEnable()
        {
            _spinButton.onClick.AddListener(SpinClickHandle);
            _backButton.onClick.AddListener(BackClickHandle);
            _dropsTotalBar.SellAllClicked += SellAllClickHandle;

            ResetDrops();
            _idleHint.gameObject.SetActive(true);
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
            var cellHeight = _slotMachineCellPrefab.DesignHeight * scale;

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

            ResetDrops();
            _idleHint.gameObject.SetActive(true);
            _dropsTotalSlot.SetActive(count > 1);

            return _activeSlotMachines;
        }

        public void StopSlotMachines()
        {
            foreach (var slotMachine in _slotMachines)
            {
                slotMachine.StopSpinAndClear();
            }

            ResetDrops();
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

        public void SetRewards(IReadOnlyList<DropResultCard.Data> rewards)
        {
            ResetDrops();
            _rewards.AddRange(rewards);

            _idleHint.gameObject.SetActive(false);
        }

        public void SetDropsTotal(int unsoldCount, float unsoldTotal, float soldTotal)
        {
            _dropsTotalBar.SetState(unsoldCount, unsoldTotal, soldTotal);
        }

        public void MarkDropSold(int index, float amount)
        {
            _dropResultCards[index].MarkSold(amount);
        }

        public void PlayDropSoldFeedback(int index)
        {
            _moneyBurst.PlayAt(_dropResultCards[index].SellButtonRect, _sellParticlesCount);
        }

        public void MarkDropUnavailable(int index)
        {
            _dropResultCards[index].MarkUnavailable();
        }

        public void PlaySellAllFeedback()
        {
            _moneyBurst.PlayAt(_dropsTotalBar.SellAllButtonRect, _sellAllParticlesCount);
        }

        private void CreateSlotMachine()
        {
            var cell = Instantiate(_slotMachineCellPrefab, _slotMachinesStack);
            var slotMachine = cell.SlotMachine;
            var dropResultCard = cell.DropResultCard;

            cell.name = $"{_slotMachineCellPrefab.name} ({_slotMachines.Count})";
            slotMachine.SpinCompleted += SpinCompleteHandle;
            dropResultCard.SellClicked += DropSellClickHandle;
            dropResultCard.Hide();

            _slotMachineCells.Add(cell.LayoutElement);
            _slotMachines.Add(slotMachine);
            _dropResultCards.Add(dropResultCard);
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

        private void ResetDrops()
        {
            _rewards.Clear();
            _completedSpinsCount = 0;

            foreach (var dropResultCard in _dropResultCards)
            {
                dropResultCard.Hide();
            }

            _dropsTotalBar.Hide();
        }

        private void SpinCompleteHandle(HorizontalSlotMachine slotMachine, Slot winSlot)
        {
            var index = _activeSlotMachines.IndexOf(slotMachine);

            _dropResultCards[index].Show(_rewards[index]);
            _completedSpinsCount++;

            if (_completedSpinsCount == _rewards.Count)
            {
                ShowAllDropsCompleted();
            }
        }

        private void ShowAllDropsCompleted()
        {
            var bestIndex = _rewards.IndexOfMax(reward => reward.SellPrice);
            var bestReward = _rewards[bestIndex];

            if (_rewards.Count > 1)
            {
                _dropResultCards[bestIndex].MarkBest(bestReward.RarityColor);
                _dropsTotalBar.Show();
            }

            _screenFlash.Play(bestReward.RarityColor);
        }

        private void DropSellClickHandle(DropResultCard dropResultCard)
        {
            DropSellClicked?.Invoke(_dropResultCards.IndexOf(dropResultCard));
        }

        private void SellAllClickHandle()
        {
            SellAllClicked?.Invoke();
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
            _dropsTotalBar.SellAllClicked -= SellAllClickHandle;
        }

        private void OnDestroy()
        {
            foreach (var slotMachine in _slotMachines)
            {
                slotMachine.SpinCompleted -= SpinCompleteHandle;
            }

            foreach (var dropResultCard in _dropResultCards)
            {
                dropResultCard.SellClicked -= DropSellClickHandle;
            }

            foreach (var countOption in _countOptions)
            {
                countOption.Clicked -= CountOptionClickHandle;
            }
        }
    }
}

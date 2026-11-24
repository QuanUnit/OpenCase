using System;
using System.Collections.Generic;
using System.Linq;
using Features.Inventory.UI.Panels;
using Features.Skins.Data;
using QCore.UISystem.Panels;
using Shared.UI;
using Shared.UI.Effects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Contracts.UI.Panels
{
    public class ContractSubPanel : Panel
    {
        public event Action<SkinInstanceId> SkinClicked;
        public event Action<int> SlotClicked;
        public event Action SignClicked;
        public event Action SignatureClearClicked;
        public event Action SignatureConfirmClicked;
        public event Action SignatureCancelClicked;
        public event Action<float> SignatureCoverageChanged;
        public event Action ResultCloseClicked;

        [Header("Inventory")]
        [SerializeField] private Transform _skinsViewport;
        [SerializeField] private SkinView _skinViewPrefab;
        [SerializeField] private GameObject _emptyState;
        [SerializeField, Range(0f, 1f)] private float _unavailableSkinAlpha = 0.3f;

        [Header("Contract")]
        [SerializeField] private ContractSlotView[] _slots;
        [SerializeField] private Transform _outcomesRoot;
        [SerializeField] private ContractOutcomeView _outcomeViewPrefab;
        [SerializeField] private GameObject _outcomesHint;
        [SerializeField] private TMP_Text _inputsCountText;
        [SerializeField] private string _inputsCountFormat = "{0} / {1}";
        [SerializeField] private TextedCurrencyView _inputsValue;
        [SerializeField] private Button _signButton;
        [SerializeField] private CanvasGroup _inputGroup;

        [Header("Signature")]
        [SerializeField] private GameObject _signaturePopup;
        [SerializeField] private SignaturePad _signaturePad;
        [SerializeField] private Button _signatureClearButton;
        [SerializeField] private Button _signatureConfirmButton;
        [SerializeField] private Button _signatureCancelButton;

        [Header("Result")]
        [SerializeField] private GameObject _resultPopup;
        [SerializeField] private ContractResultView _resultView;
        [SerializeField] private Button _resultCloseButton;
        [SerializeField] private ScreenFlash _screenFlash;

        private readonly Dictionary<SkinInstanceId, SkinView> _activeViews = new Dictionary<SkinInstanceId, SkinView>();
        private readonly Dictionary<SkinView, SkinInstanceId> _viewToSkinIdMap = new Dictionary<SkinView, SkinInstanceId>();
        private readonly List<ContractOutcomeView> _outcomeViews = new List<ContractOutcomeView>();

        public IEnumerable<SkinInstanceId> SkinIds => _activeViews.Keys;

        private void OnEnable()
        {
            for (var index = 0; index < _slots.Length; index++)
            {
                _slots[index].SetNumber(index + 1);
                _slots[index].Clicked += SlotClickHandle;
            }

            _signButton.onClick.AddListener(SignButtonClickHandle);
            _signatureClearButton.onClick.AddListener(SignatureClearButtonClickHandle);
            _signatureConfirmButton.onClick.AddListener(SignatureConfirmButtonClickHandle);
            _signatureCancelButton.onClick.AddListener(SignatureCancelButtonClickHandle);
            _resultCloseButton.onClick.AddListener(ResultCloseButtonClickHandle);
            _signaturePad.CoverageChanged += SignatureCoverageChangeHandle;
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
        }

        public void RemoveSkinView(SkinInstanceId id)
        {
            var removed = _activeViews.Remove(id, out var view);

            if (removed == false)
            {
                return;
            }

            _viewToSkinIdMap.Remove(view);

            view.Clicked -= SkinViewClickHandle;
            Destroy(view.gameObject);
        }

        public void RemoveAllSkinViews()
        {
            while (_activeViews.Count > 0)
            {
                RemoveSkinView(_activeViews.First().Key);
            }
        }

        public void SetSkinState(SkinInstanceId id, bool isInContract, bool isAvailable)
        {
            var view = _activeViews[id];
            var canvasGroup = view.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = view.gameObject.AddComponent<CanvasGroup>();
            }

            view.gameObject.SetActive(isInContract == false);
            canvasGroup.alpha = isAvailable == true ? 1f : _unavailableSkinAlpha;
            canvasGroup.interactable = isAvailable;
        }

        public void SetEmptyState(bool isEmpty)
        {
            _emptyState.SetActive(isEmpty);
        }

        public void SetSlot(int index, Sprite icon, Color rarityColor)
        {
            _slots[index].SetView(icon, rarityColor);
        }

        public void SetSlotEmpty(int index)
        {
            _slots[index].SetEmpty();
        }

        public void SetSummary(int inputsCount, int requiredCount, float inputsValue)
        {
            _inputsCountText.text = string.Format(_inputsCountFormat, inputsCount, requiredCount);
            _inputsValue.SetView(inputsValue);
        }

        public void SetOutcomes(IReadOnlyList<ContractOutcomeView.Data> outcomes)
        {
            while (_outcomeViews.Count > outcomes.Count)
            {
                var last = _outcomeViews[_outcomeViews.Count - 1];
                _outcomeViews.RemoveAt(_outcomeViews.Count - 1);
                Destroy(last.gameObject);
            }

            while (_outcomeViews.Count < outcomes.Count)
            {
                _outcomeViews.Add(Instantiate(_outcomeViewPrefab, _outcomesRoot));
            }

            for (var index = 0; index < outcomes.Count; index++)
            {
                _outcomeViews[index].SetView(outcomes[index]);
            }

            _outcomesHint.SetActive(outcomes.Count == 0);
        }

        public void SetSignInteractable(bool isInteractable)
        {
            _signButton.interactable = isInteractable;
        }

        public void SetInputBlocked(bool isBlocked)
        {
            _inputGroup.interactable = isBlocked == false;
            _inputGroup.blocksRaycasts = isBlocked == false;
        }

        public int SlotsCount => _slots.Length;

        public void ShowSignature()
        {
            _signaturePopup.SetActive(true);
            _signaturePad.Clear();
            SetSignatureButtonsInteractable(true);
            SetSignatureConfirmInteractable(false);
        }

        public void HideSignature()
        {
            _signaturePopup.SetActive(false);
        }

        public void ClearSignature()
        {
            _signaturePad.Clear();
        }

        public void SetSignatureConfirmInteractable(bool isInteractable)
        {
            _signatureConfirmButton.interactable = isInteractable;
        }

        public void SetSignatureButtonsInteractable(bool isInteractable)
        {
            _signatureClearButton.interactable = isInteractable;
            _signatureCancelButton.interactable = isInteractable;
            _signatureConfirmButton.interactable = isInteractable;
        }

        public void ShowResult(ContractResultView.Data data)
        {
            _resultPopup.SetActive(true);
            _resultView.Show(data);
            _screenFlash.Play(data.RarityColor);
        }

        public void HideResult()
        {
            _resultPopup.SetActive(false);
        }

        private void SkinViewClickHandle(SkinView sender)
        {
            SkinClicked?.Invoke(_viewToSkinIdMap[sender]);
        }

        private void SlotClickHandle(ContractSlotView sender)
        {
            SlotClicked?.Invoke(Array.IndexOf(_slots, sender));
        }

        private void SignButtonClickHandle()
        {
            SignClicked?.Invoke();
        }

        private void SignatureClearButtonClickHandle()
        {
            SignatureClearClicked?.Invoke();
        }

        private void SignatureConfirmButtonClickHandle()
        {
            SignatureConfirmClicked?.Invoke();
        }

        private void SignatureCancelButtonClickHandle()
        {
            SignatureCancelClicked?.Invoke();
        }

        private void SignatureCoverageChangeHandle(float coverage)
        {
            SignatureCoverageChanged?.Invoke(coverage);
        }

        private void ResultCloseButtonClickHandle()
        {
            ResultCloseClicked?.Invoke();
        }

        private void OnDisable()
        {
            foreach (var slot in _slots)
            {
                slot.Clicked -= SlotClickHandle;
            }

            _signButton.onClick.RemoveListener(SignButtonClickHandle);
            _signatureClearButton.onClick.RemoveListener(SignatureClearButtonClickHandle);
            _signatureConfirmButton.onClick.RemoveListener(SignatureConfirmButtonClickHandle);
            _signatureCancelButton.onClick.RemoveListener(SignatureCancelButtonClickHandle);
            _resultCloseButton.onClick.RemoveListener(ResultCloseButtonClickHandle);
            _signaturePad.CoverageChanged -= SignatureCoverageChangeHandle;
        }
    }
}

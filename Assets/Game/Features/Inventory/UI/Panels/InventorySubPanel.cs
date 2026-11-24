using System;
using System.Collections.Generic;
using System.Linq;
using Shared.UI;
using Shared.UI.SelectableSet;
using Features.Skins.Data;
using QCore.UISystem.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Inventory.UI.Panels
{
    public class InventorySubPanel : Panel
    {
        public event Action<SkinInstanceId> SkinClicked;
        public event Action SellClicked; 

        public SkinDetailsPopup SkinDetailsPopup => _skinDetailsPopup;
        public IEnumerable<SkinView> ActiveViews => _activeViews.Values;
        public SelectableSetControlBlock SelectableSetControlBlock => _selectableSetControlBlock;
        
        [SerializeField] private Transform _skinsViewport;
        [SerializeField] private SelectableSetControlBlock _selectableSetControlBlock;
        [SerializeField] private SkinView _skinViewPrefab;
        [SerializeField] private SkinDetailsPopup _skinDetailsPopup;
        [SerializeField] private Button _sellButton;
        [SerializeField] private TextedCurrencyView _sellButtonCurrency;
        [SerializeField] private TMP_Text _itemsCount;
        [SerializeField] private TextedCurrencyView _totalValue;
        [SerializeField] private GameObject _emptyState;

        private readonly Dictionary<SkinInstanceId, SkinView> _activeViews = new Dictionary<SkinInstanceId, SkinView>();
        private readonly Dictionary<SkinView, SkinInstanceId> _viewToSkinIdMap = new Dictionary<SkinView, SkinInstanceId>();

        private void OnEnable()
        {
            _sellButton.onClick.AddListener(SellClickHandle);

            UpdateSummary();
        }

        public void AddSkinViews(IEnumerable<Skin> skins, CreateSkinViewAction createAction)
        {
            foreach (var skin in skins)
            {
                AddSkinView(skin, createAction);
            }
        }
        
        public void AddSkinView(Skin skin, CreateSkinViewAction createAction)
        {
            var created = createAction.Invoke(skin, _skinViewPrefab, _skinsViewport);

            created.Clicked += SkinClickHandle;
            _activeViews.Add(skin.Id, created);
            _viewToSkinIdMap.Add(created, skin.Id);

            UpdateSummary();
        }

        private void SkinClickHandle(SkinView sender)
        {
            var skinId = GetSkinId(sender);
            SkinClicked?.Invoke(skinId);
        }

        public void RemoveSkinView(SkinInstanceId id)
        {
            var removed = _activeViews.Remove(id, out var view);

            if(removed == false) return;
            
            _viewToSkinIdMap.Remove(view);

            view.Clicked -= SkinClickHandle;
            Destroy(view.gameObject);

            UpdateSummary();
        }

        public void RemoveAllSkinViews()
        {
            while (_activeViews.Count > 0)
            {
                RemoveSkinView(_activeViews.First().Key);
            }
        }

        public SkinInstanceId GetSkinId(SkinView view)
        {
            return _viewToSkinIdMap[view];
        }

        public void SwitchEnableSellButton(bool value)
        {
            _sellButton.gameObject.SetActive(value);
        }

        public void SetSellButtonPrice(float price)
        {
            _sellButtonCurrency.SetView(price);
        }
        
        private void UpdateSummary()
        {
            var totalValue = 0f;

            foreach (var view in _activeViews.Values)
            {
                totalValue += view.Price;
            }

            _itemsCount.SetText("{0}", _activeViews.Count);
            _totalValue.SetView(totalValue);
            _emptyState.SetActive(_activeViews.Count == 0);
        }

        private void SellClickHandle()
        {
            SellClicked?.Invoke();
        }

        private void OnDisable()
        {
            _sellButton.onClick.RemoveListener(SellClickHandle);
        }

        public delegate SkinView CreateSkinViewAction(Skin skin, SkinView prefab, Transform parent);
    }
}
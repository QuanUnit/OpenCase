using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Shared.UI.SelectableSet
{
    public class SelectableSetController
    {
        public event Action<bool> SelectingModeChanged;
        public event Action<SelectableItem> SelectionChanged;
        
        public bool IsSelecting { get; private set; }
        
        private Button _startSelectionButton;
        private Button _stopSelectionButton;
        private Button _selectAllButton;
        private Button _deselectAllButton;

        private Func<IEnumerable<GameObject>> _selectableItemsGetter;
        
        private HashSet<SelectableItem> _allSelectableItems = new HashSet<SelectableItem>();
        private HashSet<SelectableItem> _selectedItems = new HashSet<SelectableItem>();

        public void Init(Data data)
        {
            _startSelectionButton = data.StartSelectingButton;
            _stopSelectionButton = data.StopSelectingButton;
            _selectAllButton = data.SelectAllButton;
            _deselectAllButton = data.DeselectAllButton;
            _selectableItemsGetter = data.SelectableItemsGetter;
            
            _startSelectionButton.onClick.AddListener(StartSelectionClickHandle);
            _stopSelectionButton.onClick.AddListener(StopSelectionClickHandle);
            _selectAllButton.onClick.AddListener(SelectAllClickHandle);
            _deselectAllButton.onClick.AddListener(DeselectAllClickHandle);
        }

        private void StartSelecting(IEnumerable<GameObject> selectableItems)
        {
            if (IsSelecting) return;
            
            IsSelecting = true;
            
            foreach (var selectableItem in selectableItems)
            {
                TryAddSelectableItem(selectableItem);
            }

            SwitchEnableSelectionControlButtons(true);
            SwitchEnableStartSelectionButton(false);
            
            SelectingModeChanged?.Invoke(true);
        }
        
        public void TryAddSelectableItem(GameObject item)
        {
            var selectableItem = item.GetComponent<SelectableItem>();

            _allSelectableItems.Add(selectableItem);
            selectableItem.SelectionChanged += SelectionChangeHandle;
            selectableItem.SwitchEnableSelectionMode(true);
            selectableItem.ForceChangeSelectedState(false);
        }

        public void TryRemoveSelectableItem(GameObject item)
        {
            TryRemoveSelectableItem(item.GetComponent<SelectableItem>());
        }
        
        public void TryRemoveSelectableItem(SelectableItem item)
        {
            _allSelectableItems.Remove(item);
            _selectedItems.Remove(item);
            
            item.SwitchEnableSelectionMode(false);
            item.SelectionChanged -= SelectionChangeHandle;
        }
        
        private void SelectionChangeHandle(SelectableItem sender, bool isSelected)
        {
            if (isSelected)
            {
                _selectedItems.Add(sender);
            }
            else
            {
                _selectedItems.Remove(sender);
            }
            
            SelectionChanged?.Invoke(sender);
        }

        public IEnumerable<SelectableItem> GetSelectedItems()
        {
            return _selectedItems;
        }

        public void StopSelecting()
        {
            if (IsSelecting == false) return;
            
            IsSelecting = false;

            while (_allSelectableItems.Count > 0)
            {
                TryRemoveSelectableItem(_allSelectableItems.First());
            }
            
            SwitchEnableSelectionControlButtons(false);
            SwitchEnableStartSelectionButton(true);
            
            SelectingModeChanged?.Invoke(false);
        }
        
        private void StartSelectionClickHandle()
        {
            StartSelecting(_selectableItemsGetter.Invoke());
        }

        private void StopSelectionClickHandle()
        {
            StopSelecting();
        }
        
        private void SelectAllClickHandle()
        {
            foreach (var item in _allSelectableItems)
            {
                if (item.IsSelected == false)
                {
                    item.ForceChangeSelectedState(true);
                }
            }
        }
        
        private void DeselectAllClickHandle()
        {
            foreach (var item in _allSelectableItems)
            {
                if (item.IsSelected == true)
                {
                    item.ForceChangeSelectedState(false);
                }
            }
        }

        private void SwitchEnableSelectionControlButtons(bool value)
        {
            _stopSelectionButton.gameObject.SetActive(value);
            _selectAllButton.gameObject.SetActive(value);
            _deselectAllButton.gameObject.SetActive(value);
        }

        private void SwitchEnableStartSelectionButton(bool value)
        {
            _startSelectionButton.gameObject.SetActive(value);
        }
        
        public void Dispose()
        {
            _startSelectionButton.onClick.RemoveListener(StartSelectionClickHandle);
            _stopSelectionButton.onClick.RemoveListener(StopSelectionClickHandle);
            _selectAllButton.onClick.RemoveListener(SelectAllClickHandle);
            _deselectAllButton.onClick.RemoveListener(DeselectAllClickHandle);

            StopSelecting();
        }
        
        public struct Data
        {
            public Button StartSelectingButton;
            public Button StopSelectingButton;
            public Button SelectAllButton;
            public Button DeselectAllButton;
            public Func<IEnumerable<GameObject>> SelectableItemsGetter;
        }
    }
}
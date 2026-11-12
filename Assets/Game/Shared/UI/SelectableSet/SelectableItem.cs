using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shared.UI.SelectableSet
{
    public class SelectableItem : MonoBehaviour
    {
        public event Action<SelectableItem, bool> SelectionChanged;
        public bool IsSelected => _toggle.isOn;
        
        [SerializeField] private Toggle _toggle;
        [SerializeField] private GameObject _selectedVisual;

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(SelectionValueChangeHandle);
            UpdateSelectedVisual();
        }

        private void SelectionValueChangeHandle(bool isSelected)
        {
            UpdateSelectedVisual();
            SelectionChanged?.Invoke(this, isSelected);
        }

        public void SwitchEnableSelectionMode(bool isSelectionMode)
        {
            _toggle.gameObject.SetActive(isSelectionMode);
        }

        public void ForceChangeSelectedState(bool isSelected)
        {
            _toggle.isOn = isSelected;
            UpdateSelectedVisual();
        }

        private void UpdateSelectedVisual()
        {
            _selectedVisual.SetActive(_toggle.isOn);
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(SelectionValueChangeHandle);
        }
    }
}
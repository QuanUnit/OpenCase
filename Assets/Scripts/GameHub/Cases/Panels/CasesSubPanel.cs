using System;
using System.Collections.Generic;
using QCore.UISystem.Panels;
using SlotMachines.Horizontal;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Cases.Panels
{
    public class CasesSubPanel : Panel
    {
        public event Action SpinClicked;
        public IReadOnlyList<HorizontalSlotMachine> HorizontalSlotMachines => _horizontalSlotMachines;
        
        [SerializeField] private List<HorizontalSlotMachine> _horizontalSlotMachines;
        [SerializeField] private Button _spinButton;

        private void OnEnable()
        {
            _spinButton.onClick.AddListener(SpinClickHandle);
        }

        private void SpinClickHandle()
        {
            SpinClicked?.Invoke();
        }

        private void OnDisable()
        {
            _spinButton.onClick.RemoveListener(SpinClickHandle);
        }
    }
}
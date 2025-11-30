using System;
using QCore.UISystem.Panels;
using SlotMachines.Horizontal;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Cases.Panels
{
    public class CaseSpinSubPanel : Panel
    {
        public event Action SpinClicked;
        public event Action BackClicked;
        
        public HorizontalSlotMachine SlotMachine => _slotMachine;
        
        [SerializeField] private HorizontalSlotMachine _slotMachine;
        [SerializeField] private Button _spinButton;
        [SerializeField] private Button _backButton;

        private void OnEnable()
        {
            _spinButton.onClick.AddListener(SpinClickHandle);
            _backButton.onClick.AddListener(BackClickHandle);
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
    }
}
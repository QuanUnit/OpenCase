using System;
using System.Diagnostics.Tracing;
using GameHub.Cases.Panels;
using QCore.UISystem.Panels;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Root.Panels
{
    public class GameHubPanel : Panel
    {
        public event Action CasesClickHandle;
        
        public CasesSubPanel CasesSubPanel => _casesSubPanel;
        
        [SerializeField] private Button _casesButton;
        [SerializeField] private CasesSubPanel _casesSubPanel;

        private void OnEnable()
        {
            _casesButton.onClick.AddListener(CasesButtonClickHandle);
        }

        private void CasesButtonClickHandle()
        {
            CasesClickHandle?.Invoke();
        }

        private void OnDisable()
        {
            _casesButton.onClick.RemoveListener(CasesButtonClickHandle);
        }
    }
}
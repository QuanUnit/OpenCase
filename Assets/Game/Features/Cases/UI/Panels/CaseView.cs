using System;
using Shared.UI;
using QCore.Tools.Identifiers;
using Features.Cases.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Features.Cases.UI.Panels
{
    public class CaseView : MonoBehaviour
    {
        public event Action<CaseView> OpenClicked;
        
        public QGuid CaseId { get; private set; }
        
        [SerializeField] private Button _openButton;
        [SerializeField] private Image _mainIcon;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TextedCurrencyView _price;
        
        public void SetData(CaseConfig caseConfig)
        {
            var uiContext = caseConfig.ContextData;

            CaseId = caseConfig.Id;
            
            _mainIcon.sprite = uiContext.SpriteSource.GetAsset();
            _name.text = caseConfig.Name;
            _price.SetView(caseConfig.Price);
        }

        private void OnEnable()
        {
            _openButton.onClick.AddListener(OpenClickHandle);
        }

        private void OpenClickHandle()
        {
            OpenClicked?.Invoke(this);
        }

        private void OnDisable()
        {
            _openButton.onClick.RemoveListener(OpenClickHandle);
        }
    }
}
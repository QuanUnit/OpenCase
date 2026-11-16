using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Cases.UI.Panels
{
    public class SpinCountOptionView : MonoBehaviour
    {
        public event Action<int> Clicked;

        public int Count { get; private set; }

        [SerializeField] private Button _button;
        [SerializeField] private Image _visual;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private string _textFormat = "x{0}";
        [SerializeField] private Color _normalFillColor = new Color32(0x1D, 0x21, 0x2C, 0xFF);
        [SerializeField] private Color _normalTextColor = new Color32(0x8A, 0x93, 0xA6, 0xFF);
        [SerializeField] private Color _selectedFillColor = new Color32(0xFF, 0x8A, 0x1F, 0xFF);
        [SerializeField] private Color _selectedTextColor = new Color32(0x0B, 0x0D, 0x12, 0xFF);

        private void OnEnable()
        {
            _button.onClick.AddListener(ClickHandle);
        }

        public void SetCount(int count)
        {
            Count = count;
            _text.text = string.Format(_textFormat, count);
        }

        public void SetSelected(bool isSelected)
        {
            _visual.color = isSelected == true ? _selectedFillColor : _normalFillColor;
            _text.color = isSelected == true ? _selectedTextColor : _normalTextColor;
        }

        public void SetInteractable(bool isInteractable)
        {
            _button.interactable = isInteractable;
        }

        private void ClickHandle()
        {
            Clicked?.Invoke(Count);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(ClickHandle);
        }
    }
}

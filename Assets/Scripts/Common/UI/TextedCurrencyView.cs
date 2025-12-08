using TMPro;
using UnityEngine;

namespace Common.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextedCurrencyView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private string _postfix = " $";
        
        public void SetView(float price)
        {
            _text.text = $"{price:F}{_postfix}";
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            _text = GetComponent<TMP_Text>();
        }

#endif
    }
}
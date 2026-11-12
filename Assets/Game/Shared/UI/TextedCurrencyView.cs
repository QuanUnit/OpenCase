using Shared.UI.Effects;
using TMPro;
using UnityEngine;

namespace Shared.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextedCurrencyView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        
        public void SetView(float price)
        {
            _text.text = NumberCounterFormatter.FormatMoney(price);
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            _text = GetComponent<TMP_Text>();
        }

#endif
    }
}

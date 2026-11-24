using Shared.UI;
using Shared.UI.Effects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Contracts.UI.Panels
{
    public class ContractResultView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TextedCurrencyView _price;
        [SerializeField] private Image _rarityStrip;
        [SerializeField] private Image _rarityGlow;
        [SerializeField, Range(0f, 1f)] private float _rarityGlowAlpha = 0.3f;
        [SerializeField] private ScalePunch _punch;

        public void Show(Data data)
        {
            _icon.sprite = data.Icon;
            _icon.enabled = data.Icon != null;
            _name.text = data.Name;
            _price.SetView(data.Price);
            _rarityStrip.color = data.RarityColor;
            _rarityGlow.color = new Color(data.RarityColor.r, data.RarityColor.g, data.RarityColor.b, _rarityGlowAlpha);

            gameObject.SetActive(true);
            _punch.Play();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public struct Data
        {
            public Sprite Icon;
            public string Name;
            public float Price;
            public Color RarityColor;
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Cases.UI.Panels
{
    public class CaseContentItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _name;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _rarity;
        [SerializeField] private Image _rarityGlow;
        [SerializeField, Range(0f, 1f)] private float _rarityGlowAlpha = 0.3f;

        public void SetView(Data data)
        {
            _name.text = data.Name;
            _icon.sprite = data.Icon;
            _icon.enabled = data.Icon != null;
            _rarity.color = data.RarityColor;
            _rarityGlow.color = new Color(data.RarityColor.r, data.RarityColor.g, data.RarityColor.b, _rarityGlowAlpha);
        }

        public struct Data
        {
            public string Name;
            public Sprite Icon;
            public Color RarityColor;
        }
    }
}

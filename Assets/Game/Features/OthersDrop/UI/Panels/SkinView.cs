using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Features.OthersDrop.UI.Panels
{
    public class SkinView : MonoBehaviour
    {
        [FormerlySerializedAs("_rarityBackground")]
        [SerializeField] private Image _rarityGlow;
        [SerializeField] private Image _rarityBar;
        [SerializeField] private Image _skinIcon;
        [SerializeField] private TMP_Text _name;
        [SerializeField, Range(0f, 1f)] private float _rarityGlowAlpha = 0.3f;

        public void SetView(Data data)
        {
            _name.text = data.Name;
            _skinIcon.sprite = data.SkinIcon;
            _skinIcon.enabled = data.SkinIcon != null;
            _rarityBar.color = data.RarityBar;

            var glowColor = data.RarityBar;
            glowColor.a = _rarityGlowAlpha;
            _rarityGlow.color = glowColor;
        }
        
        public struct Data
        {
            public string Name;
            public Color RarityBar;
            public Sprite SkinIcon;
        }
    }
}

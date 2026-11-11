using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.OthersDrop.UI.Panels
{
    public class SkinView : MonoBehaviour
    {
        [SerializeField] private Image _rarityBackground;
        [SerializeField] private Image _rarityBar;
        [SerializeField] private Image _skinIcon;
        [SerializeField] private TMP_Text _name;

        public void SetView(Data data)
        {
            _name.text = data.Name;
            _skinIcon.sprite = data.SkinIcon;
            _rarityBackground.color = data.RarityBackground;
            _rarityBar.color = data.RarityBar;
        }
        
        public struct Data
        {
            public string Name;
            public Color RarityBackground;
            public Color RarityBar;
            public Sprite SkinIcon;
        }
    }
}
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameHub.Inventory.Panels
{
    public class SkinView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TextedCurrencyView _price;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _rarity;
        
        public void SetView(Data data)
        {
            _name.text = data.Name;
            _price.SetView(data.Price);
            _icon.sprite = data.Icon;
            _rarity.color = data.RarityColor;
        }

        public struct Data
        {
            public string Name;
            public Sprite Icon;
            public Color RarityColor;
            public float Price;
        }
    }
}
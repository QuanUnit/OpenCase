using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Contracts.UI.Panels
{
    public class ContractOutcomeView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _chance;
        [SerializeField] private Image _rarityStrip;
        [SerializeField] private string _chanceFormat = "{0:0.0}%";

        public void SetView(Data data)
        {
            _icon.sprite = data.Icon;
            _icon.enabled = data.Icon != null;
            _name.text = data.Name;
            _chance.text = string.Format(CultureInfo.InvariantCulture, _chanceFormat, data.Chance * 100f);
            _rarityStrip.color = data.RarityColor;
        }

        public struct Data
        {
            public Sprite Icon;
            public string Name;
            public float Chance;
            public Color RarityColor;
        }
    }
}

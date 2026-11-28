using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace Features.Upgrade.UI.Panels
{
    public class UpgradeSourceStack : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _layers;
        [SerializeField] private GameObject _badge;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private string _countFormat = "x{0}";

        public void SetCount(int count)
        {
            for (var index = 0; index < _layers.Count; index++)
            {
                _layers[index].SetActive(count > index + 1);
            }

            _badge.SetActive(count > 1);
            _countText.text = string.Format(CultureInfo.InvariantCulture, _countFormat, count);
        }
    }
}

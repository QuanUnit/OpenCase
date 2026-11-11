using TMPro;
using UnityEngine;

namespace Features.Cases.UI.Panels
{
    public class CasesCategoryTitle : MonoBehaviour
    {
        [SerializeField] private TMP_Text _name;

        public void SetData(string name)
        {
            _name.text = name;
        }
    }
}
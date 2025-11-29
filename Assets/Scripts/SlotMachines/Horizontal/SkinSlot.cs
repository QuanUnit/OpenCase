using Services.Skins.Configs;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachines.Horizontal
{
    public class SkinSlot : Slot
    {
        [SerializeField] private Image _skinIcon;
        [SerializeField] private Image _rarityImage;

        public void SetData(Sprite skinIcon, RarityVisual rarityVisual)
        {
            _skinIcon.sprite = skinIcon;
            _rarityImage.color = rarityVisual.RarityMainColor;
        }
    }
}
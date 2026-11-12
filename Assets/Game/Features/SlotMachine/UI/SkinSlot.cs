using Features.Skins.Configs;
using Shared.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace Features.SlotMachine.UI
{
    public class SkinSlot : Slot
    {
        [SerializeField] private Image _skinIcon;
        [SerializeField] private Image _rarityImage;
        [SerializeField] private Image _rarityGlow;
        [SerializeField, Range(0f, 1f)] private float _rarityGlowAlpha = 0.3f;
        [SerializeField] private Image _winOutline;
        [SerializeField] private PulseGlow _winGlow;
        [SerializeField] private ScalePunch _winPunch;

        public void SetData(Sprite skinIcon, RarityVisual rarityVisual)
        {
            var rarityColor = rarityVisual.RarityMainColor;

            _skinIcon.sprite = skinIcon;
            _skinIcon.enabled = skinIcon != null;
            _rarityImage.color = rarityColor;
            _rarityGlow.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, _rarityGlowAlpha);
            _winOutline.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, _winOutline.color.a);
            _winOutline.gameObject.SetActive(false);
        }

        public override void PlayWin()
        {
            _winOutline.gameObject.SetActive(true);
            _winGlow.Play();
            _winPunch.Play();
        }
    }
}

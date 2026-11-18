using DG.Tweening;
using Shared.UI;
using Shared.UI.Effects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Upgrade.UI.Panels
{
    public class UpgradeSkinCard : MonoBehaviour
    {
        [SerializeField] private GameObject _filled;
        [SerializeField] private GameObject _placeholder;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TextedCurrencyView _price;
        [SerializeField] private Image _rarityStrip;
        [SerializeField] private Image _rarityGlow;
        [SerializeField, Range(0f, 1f)] private float _rarityGlowAlpha = 0.3f;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField, Range(0f, 1f)] private float _lostAlpha = 0.25f;
        [SerializeField, Min(0f)] private float _lostFadeDuration = 0.3f;
        [SerializeField] private ScalePunch _winPunch;

        private Tween _fadeTween;

        public void SetView(Data data)
        {
            _fadeTween?.Kill();
            _canvasGroup.alpha = 1f;

            _icon.sprite = data.Icon;
            _icon.enabled = data.Icon != null;
            _name.text = data.Name;
            _price.SetView(data.Price);
            _rarityStrip.color = data.RarityColor;
            _rarityGlow.color = new Color(data.RarityColor.r, data.RarityColor.g, data.RarityColor.b, _rarityGlowAlpha);

            _filled.SetActive(true);
            _placeholder.SetActive(false);
        }

        public void SetEmpty()
        {
            _fadeTween?.Kill();
            _canvasGroup.alpha = 1f;

            _filled.SetActive(false);
            _placeholder.SetActive(true);
        }

        public void PlayWin()
        {
            _winPunch.Play();
        }

        public void PlayLost()
        {
            _fadeTween?.Kill();
            _fadeTween = DOVirtual
                .Float(_canvasGroup.alpha, _lostAlpha, _lostFadeDuration, AlphaUpdateHandle)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        private void AlphaUpdateHandle(float alpha)
        {
            _canvasGroup.alpha = alpha;
        }

        private void OnDisable()
        {
            _fadeTween?.Kill();
            _canvasGroup.alpha = 1f;
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

using System;
using TMPro;
using UnityEngine;

namespace Shared.UI.Theme
{
    [CreateAssetMenu(menuName = "Configs/Shared/" + nameof(UiTheme), fileName = nameof(UiTheme))]
    public class UiTheme : ScriptableObject
    {
        public Color Background => _background;
        public Color Surface => _surface;
        public Color SurfaceRaised => _surfaceRaised;
        public Color Stroke => _stroke;
        public Color TextPrimary => _textPrimary;
        public Color TextMuted => _textMuted;
        public Color Accent => _accent;
        public Color AccentLight => _accentLight;
        public Color Success => _success;
        public Color Danger => _danger;
        public Color Overlay => _overlay;

        public TMP_FontAsset BodyFont => _bodyFont;
        public TMP_FontAsset HeadingFont => _headingFont;
        public float Heading1Size => _heading1Size;
        public float Heading1CharacterSpacing => _heading1CharacterSpacing;
        public float Heading2Size => _heading2Size;
        public float BodySize => _bodySize;
        public float CaptionSize => _captionSize;
        public float NumberSize => _numberSize;

        public Sprite RoundedRect12 => _roundedRect12;
        public Sprite RoundedRect16 => _roundedRect16;
        public Sprite RoundedRect24 => _roundedRect24;
        public Sprite Pill => _pill;
        public Sprite RoundedOutline16 => _roundedOutline16;
        public Sprite SoftGlow => _softGlow;
        public Sprite VerticalGradient => _verticalGradient;
        public Sprite Circle => _circle;

        public Sprite CoinIcon => _coinIcon;
        public Sprite BoxIcon => _boxIcon;
        public Sprite GridIcon => _gridIcon;
        public Sprite CloseIcon => _closeIcon;
        public Sprite CheckIcon => _checkIcon;
        public Sprite CaseIcon => _caseIcon;

        public float SpacingSmall => _spacingSmall;
        public float SpacingMedium => _spacingMedium;
        public float SpacingLarge => _spacingLarge;
        public float RarityStripThickness => _rarityStripThickness;
        public float RarityGlowAlpha => _rarityGlowAlpha;

        public float FastDuration => _fastDuration;
        public float BaseDuration => _baseDuration;
        public float CounterDuration => _counterDuration;
        public float WinPunchScale => _winPunchScale;
        public float WinPunchDuration => _winPunchDuration;
        public float ScreenFlashAlpha => _screenFlashAlpha;
        public float ScreenFlashDuration => _screenFlashDuration;

        [Header("Colors")]
        [SerializeField] private Color _background = new Color32(0x0B, 0x0D, 0x12, 0xFF);
        [SerializeField] private Color _surface = new Color32(0x15, 0x18, 0x21, 0xFF);
        [SerializeField] private Color _surfaceRaised = new Color32(0x1D, 0x21, 0x2C, 0xFF);
        [SerializeField] private Color _stroke = new Color32(0x2A, 0x2F, 0x3D, 0xFF);
        [SerializeField] private Color _textPrimary = new Color32(0xF2, 0xF4, 0xF8, 0xFF);
        [SerializeField] private Color _textMuted = new Color32(0x8A, 0x93, 0xA6, 0xFF);
        [SerializeField] private Color _accent = new Color32(0xFF, 0x8A, 0x1F, 0xFF);
        [SerializeField] private Color _accentLight = new Color32(0xFF, 0xB5, 0x47, 0xFF);
        [SerializeField] private Color _success = new Color32(0x3D, 0xDC, 0x84, 0xFF);
        [SerializeField] private Color _danger = new Color32(0xFF, 0x4D, 0x5E, 0xFF);
        [SerializeField] private Color _overlay = new Color(0f, 0f, 0f, 0.85f);

        [Header("Typography")]
        [SerializeField] private TMP_FontAsset _bodyFont;
        [SerializeField] private TMP_FontAsset _headingFont;
        [SerializeField] private float _heading1Size = 44f;
        [SerializeField] private float _heading1CharacterSpacing = 2f;
        [SerializeField] private float _heading2Size = 30f;
        [SerializeField] private float _bodySize = 22f;
        [SerializeField] private float _captionSize = 16f;
        [SerializeField] private float _numberSize = 24f;

        [Header("Shapes")]
        [SerializeField] private Sprite _roundedRect12;
        [SerializeField] private Sprite _roundedRect16;
        [SerializeField] private Sprite _roundedRect24;
        [SerializeField] private Sprite _pill;
        [SerializeField] private Sprite _roundedOutline16;
        [SerializeField] private Sprite _softGlow;
        [SerializeField] private Sprite _verticalGradient;
        [SerializeField] private Sprite _circle;

        [Header("Icons")]
        [SerializeField] private Sprite _coinIcon;
        [SerializeField] private Sprite _boxIcon;
        [SerializeField] private Sprite _gridIcon;
        [SerializeField] private Sprite _closeIcon;
        [SerializeField] private Sprite _checkIcon;
        [SerializeField] private Sprite _caseIcon;

        [Header("Layout")]
        [SerializeField] private float _spacingSmall = 16f;
        [SerializeField] private float _spacingMedium = 24f;
        [SerializeField] private float _spacingLarge = 32f;
        [SerializeField] private float _rarityStripThickness = 4f;
        [SerializeField, Range(0f, 1f)] private float _rarityGlowAlpha = 0.3f;

        [Header("Motion")]
        [SerializeField] private float _fastDuration = 0.12f;
        [SerializeField] private float _baseDuration = 0.25f;
        [SerializeField] private float _counterDuration = 0.4f;
        [SerializeField] private float _winPunchScale = 1.12f;
        [SerializeField] private float _winPunchDuration = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _screenFlashAlpha = 0.025f;
        [SerializeField] private float _screenFlashDuration = 0.4f;

        public Color GetColor(UiColorToken token)
        {
            switch (token)
            {
                case UiColorToken.Background:
                    return _background;
                case UiColorToken.Surface:
                    return _surface;
                case UiColorToken.SurfaceRaised:
                    return _surfaceRaised;
                case UiColorToken.Stroke:
                    return _stroke;
                case UiColorToken.TextPrimary:
                    return _textPrimary;
                case UiColorToken.TextMuted:
                    return _textMuted;
                case UiColorToken.Accent:
                    return _accent;
                case UiColorToken.AccentLight:
                    return _accentLight;
                case UiColorToken.Success:
                    return _success;
                case UiColorToken.Danger:
                    return _danger;
                case UiColorToken.Overlay:
                    return _overlay;
                default:
                    throw new ArgumentOutOfRangeException(nameof(token), token, null);
            }
        }

        public Color GetColor(UiColorToken token, float alpha)
        {
            var color = GetColor(token);
            color.a = alpha;

            return color;
        }
    }
}

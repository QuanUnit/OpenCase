using System;
using NUnit.Framework;
using Shared.UI.Theme;
using UnityEngine;

namespace Shared.Tests.EditMode
{
    public class UiThemeTests
    {
        private UiTheme _theme;

        [SetUp]
        public void SetUp()
        {
            _theme = ScriptableObject.CreateInstance<UiTheme>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_theme);
        }

        [Test]
        public void GetColor_EveryToken_MatchesProperty()
        {
            Assert.AreEqual(_theme.Background, _theme.GetColor(UiColorToken.Background));
            Assert.AreEqual(_theme.Surface, _theme.GetColor(UiColorToken.Surface));
            Assert.AreEqual(_theme.SurfaceRaised, _theme.GetColor(UiColorToken.SurfaceRaised));
            Assert.AreEqual(_theme.Stroke, _theme.GetColor(UiColorToken.Stroke));
            Assert.AreEqual(_theme.TextPrimary, _theme.GetColor(UiColorToken.TextPrimary));
            Assert.AreEqual(_theme.TextMuted, _theme.GetColor(UiColorToken.TextMuted));
            Assert.AreEqual(_theme.Accent, _theme.GetColor(UiColorToken.Accent));
            Assert.AreEqual(_theme.AccentLight, _theme.GetColor(UiColorToken.AccentLight));
            Assert.AreEqual(_theme.Success, _theme.GetColor(UiColorToken.Success));
            Assert.AreEqual(_theme.Danger, _theme.GetColor(UiColorToken.Danger));
            Assert.AreEqual(_theme.Overlay, _theme.GetColor(UiColorToken.Overlay));
        }

        [Test]
        public void GetColor_EveryToken_DoesNotThrow()
        {
            foreach (UiColorToken token in Enum.GetValues(typeof(UiColorToken)))
            {
                Assert.DoesNotThrow(() => _theme.GetColor(token));
            }
        }

        [Test]
        public void GetColor_Defaults_MatchSpecHex()
        {
            Assert.AreEqual("FF8A1FFF", ColorUtility.ToHtmlStringRGBA(_theme.GetColor(UiColorToken.Accent)));
            Assert.AreEqual("0B0D12FF", ColorUtility.ToHtmlStringRGBA(_theme.GetColor(UiColorToken.Background)));
            Assert.AreEqual("3DDC84FF", ColorUtility.ToHtmlStringRGBA(_theme.GetColor(UiColorToken.Success)));
        }

        [Test]
        public void GetColor_WithAlpha_OverridesOnlyAlpha()
        {
            var color = _theme.GetColor(UiColorToken.Accent, 0.12f);
            var accent = _theme.Accent;

            Assert.AreEqual(0.12f, color.a, 0.0001f);
            Assert.AreEqual(accent.r, color.r, 0.0001f);
            Assert.AreEqual(accent.g, color.g, 0.0001f);
            Assert.AreEqual(accent.b, color.b, 0.0001f);
        }
    }
}

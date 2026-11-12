using NUnit.Framework;
using Shared.UI.Theme;
using UnityEditor;
using UnityEngine;

namespace Shared.Tests.EditMode
{
    public class UiThemeAssetTests
    {
        private const string ThemePath = "Assets/Game/Shared/Content/Configs/UiTheme.asset";

        private UiTheme _theme;

        [SetUp]
        public void SetUp()
        {
            _theme = AssetDatabase.LoadAssetAtPath<UiTheme>(ThemePath);
        }

        [Test]
        public void Asset_AtConfigsPath_Exists()
        {
            Assert.IsTrue(_theme != null);
        }

        [TestCase(UiColorToken.Background, "0B0D12FF")]
        [TestCase(UiColorToken.Surface, "151821FF")]
        [TestCase(UiColorToken.SurfaceRaised, "1D212CFF")]
        [TestCase(UiColorToken.Stroke, "2A2F3DFF")]
        [TestCase(UiColorToken.TextPrimary, "F2F4F8FF")]
        [TestCase(UiColorToken.TextMuted, "8A93A6FF")]
        [TestCase(UiColorToken.Accent, "FF8A1FFF")]
        [TestCase(UiColorToken.AccentLight, "FFB547FF")]
        [TestCase(UiColorToken.Success, "3DDC84FF")]
        [TestCase(UiColorToken.Danger, "FF4D5EFF")]
        [TestCase(UiColorToken.Overlay, "000000D9")]
        public void GetColor_Token_MatchesSpecHex(UiColorToken token, string expected)
        {
            var color = _theme.GetColor(token);

            Assert.AreEqual(expected, ColorUtility.ToHtmlStringRGBA(color));
        }

        [Test]
        public void Fonts_Assigned_AreRubik()
        {
            Assert.IsTrue(_theme.BodyFont != null);
            Assert.IsTrue(_theme.HeadingFont != null);
            Assert.AreEqual("RubikMedium SDF", _theme.BodyFont.name);
            Assert.AreEqual("RubikBold SDF", _theme.HeadingFont.name);
        }

        [Test]
        public void Sprites_All_AreAssigned()
        {
            var sprites = new[]
            {
                _theme.RoundedRect12, _theme.RoundedRect16, _theme.RoundedRect24, _theme.Pill,
                _theme.RoundedOutline16, _theme.SoftGlow, _theme.VerticalGradient, _theme.Circle,
                _theme.CoinIcon, _theme.BoxIcon, _theme.GridIcon, _theme.CloseIcon, _theme.CheckIcon, _theme.CaseIcon
            };

            foreach (var sprite in sprites)
            {
                Assert.IsTrue(sprite != null);
            }
        }

        [Test]
        public void TypeScale_Values_MatchSpec()
        {
            Assert.AreEqual(44f, _theme.Heading1Size);
            Assert.AreEqual(2f, _theme.Heading1CharacterSpacing);
            Assert.AreEqual(30f, _theme.Heading2Size);
            Assert.AreEqual(22f, _theme.BodySize);
            Assert.AreEqual(16f, _theme.CaptionSize);
            Assert.AreEqual(24f, _theme.NumberSize);
        }

        [Test]
        public void SlicedSprites_Borders_MatchRadius()
        {
            Assert.AreEqual(new Vector4(12f, 12f, 12f, 12f), _theme.RoundedRect12.border);
            Assert.AreEqual(new Vector4(16f, 16f, 16f, 16f), _theme.RoundedRect16.border);
            Assert.AreEqual(new Vector4(24f, 24f, 24f, 24f), _theme.RoundedRect24.border);
            Assert.AreEqual(new Vector4(32f, 32f, 32f, 32f), _theme.Pill.border);
            Assert.AreEqual(new Vector4(16f, 16f, 16f, 16f), _theme.RoundedOutline16.border);
        }
    }
}

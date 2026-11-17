using Features.RemoteConfigs.Editor.Exporters.Skins;
using Features.RemoteConfigs.Editor.Icons;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class SkinIconFileNamesTests
    {
        [Test]
        public void ToFileName_UnicodeAndBrackets_KeptValid()
        {
            Assert.That(SkinIconFileNames.ToFileName("M4A4_龍王_(Dragon_King)"), Is.EqualTo("M4A4_龍王_(Dragon_King).png"));
        }

        [Test]
        public void ToFileName_InvalidCharacters_ReplacedWithUnderscore()
        {
            Assert.That(SkinIconFileNames.ToFileName("AK-47_A/B:C*D?E\"F<G>H|I\\J"), Is.EqualTo("AK-47_A_B_C_D_E_F_G_H_I_J.png"));
        }

        [Test]
        public void ToFolderName_SpacesAndInvalidCharacters_Removed()
        {
            Assert.That(SkinIconFileNames.ToFolderName("Zeus x27"), Is.EqualTo("Zeusx27"));
            Assert.That(SkinIconFileNames.ToFolderName("Zeus_x27"), Is.EqualTo("Zeusx27"));
            Assert.That(SkinIconFileNames.ToFolderName("R8/Revolver"), Is.EqualTo("R8Revolver"));
        }

        [Test]
        public void IsIconOf_BrowserPrefixAndCase_Ignored()
        {
            Assert.That(SkinIconFileNames.IsIconOf("Assets/W/CZ75-Auto/imgi_60_CZ75-Auto_Tread_Plate.png", "cz75-auto_tread_plate"), Is.True);
            Assert.That(SkinIconFileNames.IsIconOf("Assets/W/AK47/AK-47_Redline.png", "AK-47_Redline"), Is.True);
        }

        [Test]
        public void IsIconOf_OtherSkin_False()
        {
            Assert.That(SkinIconFileNames.IsIconOf("Assets/W/AK47/imgi_121_AK-47_Predator.png", "M249_Predator"), Is.False);
        }

        [Test]
        public void IsIconOf_DecomposedUnicode_Matches()
        {
            var decomposedPath = "Assets/W/Negev/imgi_36_Negev_Mjölnir.png";

            Assert.That(SkinIconFileNames.IsIconOf(decomposedPath, "Negev_Mjölnir"), Is.True);
        }

        [Test]
        public void GetWeapon_IdWithName_ReturnsWeapon()
        {
            Assert.That(SkinIds.GetWeapon("SSG_08_Blood_in_the_Water", "Blood in the Water"), Is.EqualTo("SSG_08"));
            Assert.That(SkinIds.GetWeapon("Glock-18_Gamma_Doppler_Phase_4", "Gamma Doppler Phase 4"), Is.EqualTo("Glock-18"));
            Assert.That(SkinIds.GetWeapon("Broken", "Other"), Is.EqualTo("Broken"));
        }
    }
}

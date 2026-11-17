using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Icons;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class SkinIconFolderResolverTests
    {
        private const string Root = "Assets/Weapons";

        [Test]
        public void Resolve_WeaponNamesDifferFromFolders_FolderTakenFromExistingIcons()
        {
            var configs = new List<SkinIconConfigState>
            {
                new SkinIconConfigState("AK-47_Redline", "AK-47", $"{Root}/AK47/imgi_1_AK-47_Redline.png", "c1"),
                new SkinIconConfigState("Desert_Eagle_Blaze", "Desert_Eagle", $"{Root}/DesertEagle/imgi_2_Desert_Eagle_Blaze.png", "c2"),
                new SkinIconConfigState("SSG_08_Abyss", "SSG_08", $"{Root}/SSG-08/imgi_3_SSG_08_Abyss.png", "c3"),
                new SkinIconConfigState("USP-S_Kill_Confirmed", "USP-S", $"{Root}/USP/imgi_4_USP-S_Kill_Confirmed.png", "c4"),
                new SkinIconConfigState("R8_Revolver_Fade", "R8_Revolver", $"{Root}/Revolver/imgi_5_R8_Revolver_Fade.png", "c5"),
                new SkinIconConfigState("AK-47_Asiimov", "AK-47", null, "c6"),
            };

            var folders = new SkinIconFolderResolver(Root).Resolve(configs);

            Assert.That(folders["AK-47"], Is.EqualTo($"{Root}/AK47"));
            Assert.That(folders["Desert_Eagle"], Is.EqualTo($"{Root}/DesertEagle"));
            Assert.That(folders["SSG_08"], Is.EqualTo($"{Root}/SSG-08"));
            Assert.That(folders["USP-S"], Is.EqualTo($"{Root}/USP"));
            Assert.That(folders["R8_Revolver"], Is.EqualTo($"{Root}/Revolver"));
        }

        [Test]
        public void Resolve_IconsInSeveralFolders_MajorityWins()
        {
            var configs = new List<SkinIconConfigState>
            {
                new SkinIconConfigState("AK-47_A", "AK-47", $"{Root}/AK47/AK-47_A.png", "c1"),
                new SkinIconConfigState("AK-47_B", "AK-47", $"{Root}/AK47/AK-47_B.png", "c2"),
                new SkinIconConfigState("AK-47_C", "AK-47", $"{Root}/Other/AK-47_C.png", "c3"),
            };

            var folders = new SkinIconFolderResolver(Root).Resolve(configs);

            Assert.That(folders["AK-47"], Is.EqualTo($"{Root}/AK47"));
        }

        [Test]
        public void Resolve_OnlyMismatchedIcons_NotUsedAsFolder()
        {
            var configs = new List<SkinIconConfigState>
            {
                new SkinIconConfigState("Zeus_x27_Swamp_DDPAT", "Zeus_x27", $"{Root}/AWP/imgi_93_AWP_Pink_DDPAT.png", "c1"),
            };

            var folders = new SkinIconFolderResolver(Root).Resolve(configs);

            Assert.That(folders["Zeus_x27"], Is.EqualTo($"{Root}/Zeusx27"));
        }

        [Test]
        public void Resolve_NewWeapon_FolderNamedAfterWeapon()
        {
            var configs = new List<SkinIconConfigState>
            {
                new SkinIconConfigState("Zeus x27_Olympus", "Zeus x27", null, "c1"),
            };

            var folders = new SkinIconFolderResolver(Root + "/").Resolve(configs);

            Assert.That(folders["Zeus x27"], Is.EqualTo($"{Root}/Zeusx27"));
        }
    }
}

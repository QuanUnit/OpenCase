using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Icons;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class SkinIconWeaponResolverTests
    {
        [Test]
        public void Resolve_NameMatchesId_WeaponFromId()
        {
            var weapons = new SkinIconWeaponResolver().Resolve(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Desert_Eagle_Blaze", "Blaze"),
                new KeyValuePair<string, string>("Glock-18_Gamma_Doppler_Phase_4", "Gamma Doppler Phase 4"),
            });

            Assert.That(weapons, Is.EqualTo(new[] { "Desert_Eagle", "Glock-18" }));
        }

        [Test]
        public void Resolve_NameDiffersFromId_LongestKnownWeaponPrefixUsed()
        {
            var weapons = new SkinIconWeaponResolver().Resolve(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Desert_Eagle_Blaze", "Blaze"),
                new KeyValuePair<string, string>("Desert_Eagle_Blu_Ply", "Blue Ply"),
                new KeyValuePair<string, string>("Unknown_Thing", "Other"),
            });

            Assert.That(weapons, Is.EqualTo(new[] { "Desert_Eagle", "Desert_Eagle", "Unknown_Thing" }));
        }
    }
}

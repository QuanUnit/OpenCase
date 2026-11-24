using Features.Skins.Configs;
using NUnit.Framework;
using UnityEngine;

namespace Features.Skins.Tests.EditMode
{
    public class SkinConfigTests
    {
        private SkinConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<SkinConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [TestCase("AK-47_Asiimov", "Asiimov", "AK-47")]
        [TestCase("Desert_Eagle_Sunset_Storm", "Sunset Storm", "Desert Eagle")]
        [TestCase("M4A1-S_Hyper_Beast", "Hyper Beast", "M4A1-S")]
        public void WeaponName_IdEndsWithName_ReturnsWeaponWithSpaces(string skinStringId, string name, string expected)
        {
            _config.SkinStringId = skinStringId;
            _config.Name = name;

            Assert.AreEqual(expected, _config.WeaponName);
        }

        [TestCase("Desert_Eagle_Blu_Ply", "Blue Ply")]
        [TestCase("Asiimov", "Asiimov")]
        public void WeaponName_IdDoesNotEndWithName_ReturnsEmpty(string skinStringId, string name)
        {
            _config.SkinStringId = skinStringId;
            _config.Name = name;

            Assert.AreEqual(string.Empty, _config.WeaponName);
        }
    }
}

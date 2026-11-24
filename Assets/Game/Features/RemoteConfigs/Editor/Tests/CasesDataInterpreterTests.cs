using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Interpreters;
using Features.RemoteConfigs.Editor.Models;
using Features.Skins.Configs;
using NUnit.Framework;
using UnityEngine;
using Features.Skins.Data;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class CasesDataInterpreterTests
    {
        private SkinConfig _knownSkin;

        [SetUp]
        public void SetUp()
        {
            _knownSkin = ScriptableObject.CreateInstance<SkinConfig>();
            _knownSkin.Id = new SkinId("AK-47_Redline");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_knownSkin);
        }

        [Test]
        public void BuildSkinsMap_UnknownSkinId_SkippedAndReported()
        {
            var skinsById = new Dictionary<string, SkinConfig> { { _knownSkin.Id.Value, _knownSkin } };
            var caseDataModel = new CaseDataModel
            {
                Id = "Case_1",
                SkinsMap = new Dictionary<string, float> { { "AK-47_Redline", 2f }, { "AK-47_Typo", 3f } },
            };
            var report = new ImportReport();

            var skinsMap = new CasesDataInterpreter().BuildSkinsMap(caseDataModel, skinsById, report);

            Assert.That(skinsMap.Entries.Count, Is.EqualTo(1));
            Assert.That(skinsMap.Entries[0].Key, Is.SameAs(_knownSkin));
            Assert.That(skinsMap.Entries[0].Value, Is.EqualTo(2f));
            Assert.That(report.UnknownReferences, Is.EqualTo(new[] { "Case_1: AK-47_Typo" }));
        }
    }
}

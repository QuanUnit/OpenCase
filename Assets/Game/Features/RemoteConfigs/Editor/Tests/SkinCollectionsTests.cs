using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Collections;
using Features.RemoteConfigs.Editor.Exporters.Skins;
using Features.Skins.Configs;
using NUnit.Framework;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class SkinCollectionsTests
    {
        private SkinCollectionsMapper _mapper;
        private SkinCollectionsApplier _applier;
        private readonly List<SkinConfig> _createdConfigs = new List<SkinConfig>();

        [SetUp]
        public void SetUp()
        {
            _mapper = new SkinCollectionsMapper();
            _applier = new SkinCollectionsApplier();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var config in _createdConfigs)
            {
                Object.DestroyImmediate(config);
            }

            _createdConfigs.Clear();
        }

        [Test]
        public void Map_SkinWithCollection_MapsIdToCollectionName()
        {
            var skins = new List<SourceSkin> { CreateSkin("AK-47", "AK-47 | Redline", null, "The Phoenix Collection", "Other") };

            var collections = _mapper.Map(skins);

            Assert.That(collections["AK-47_Redline"], Is.EqualTo("The Phoenix Collection"));
        }

        [Test]
        public void Map_SkinWithoutCollections_IsSkipped()
        {
            var skins = new List<SourceSkin> { CreateSkin("AK-47", "AK-47 | Redline", null) };

            var collections = _mapper.Map(skins);

            Assert.That(collections.ContainsKey("AK-47_Redline"), Is.False);
        }

        [Test]
        public void Map_DopplerPhase_AddsPhaseIdAndFallback()
        {
            var skins = new List<SourceSkin> { CreateSkin("MP9", "MP9 | Doppler", "Phase 1", "The Chroma Collection") };

            var collections = _mapper.Map(skins);

            Assert.That(collections["MP9_Doppler_Phase_1"], Is.EqualTo("The Chroma Collection"));
            Assert.That(collections["MP9_Doppler"], Is.EqualTo("The Chroma Collection"));
        }

        [Test]
        public void Apply_KnownId_AssignsCollection()
        {
            var config = CreateConfig("AK-47_Redline", string.Empty);
            var collections = new Dictionary<string, string> { { "AK-47_Redline", "The Phoenix Collection" } };

            var report = _applier.Apply(new[] { config }, collections);

            Assert.That(config.Collection, Is.EqualTo("The Phoenix Collection"));
            Assert.That(report.Assigned, Is.EqualTo(new[] { "AK-47_Redline" }));
        }

        [Test]
        public void Apply_SameValue_ReportsUnchangedWithoutDirty()
        {
            var config = CreateConfig("AK-47_Redline", "The Phoenix Collection");
            var collections = new Dictionary<string, string> { { "AK-47_Redline", "The Phoenix Collection" } };

            var report = _applier.Apply(new[] { config }, collections);

            Assert.That(report.Unchanged, Is.EqualTo(new[] { "AK-47_Redline" }));
            Assert.That(report.Assigned, Is.Empty);
        }

        [Test]
        public void Apply_UnknownId_ReportsMissing()
        {
            var config = CreateConfig("AWP_Unknown", string.Empty);

            var report = _applier.Apply(new[] { config }, new Dictionary<string, string>());

            Assert.That(report.Missing, Is.EqualTo(new[] { "AWP_Unknown" }));
            Assert.That(config.Collection, Is.EqualTo(string.Empty));
        }

        private SkinConfig CreateConfig(string id, string collection)
        {
            var config = ScriptableObject.CreateInstance<SkinConfig>();
            config.SkinStringId = id;
            config.Collection = collection;
            _createdConfigs.Add(config);

            return config;
        }

        private SourceSkin CreateSkin(string weapon, string name, string phase, params string[] collectionNames)
        {
            var collections = new SourceNamedEntity[collectionNames.Length];

            for (var index = 0; index < collectionNames.Length; index++)
            {
                collections[index] = new SourceNamedEntity { name = collectionNames[index] };
            }

            return new SourceSkin
            {
                name = name,
                weapon = new SourceNamedEntity { name = weapon },
                phase = phase,
                collections = collections,
            };
        }
    }
}

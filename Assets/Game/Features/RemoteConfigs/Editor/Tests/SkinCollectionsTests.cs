using System.Collections.Generic;
using System.Linq;
using Features.RemoteConfigs.Editor.Collections;
using Features.RemoteConfigs.Editor.Exporters.Skins;
using Features.Skins.Configs;
using NUnit.Framework;
using UnityEngine;
using Features.Skins.Data;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class SkinCollectionsTests
    {
        private SkinCollectionsMapper _mapper;
        private SkinCollectionsApplier _applier;
        private SkinCollectionsConfig _registry;
        private readonly List<SkinConfig> _createdConfigs = new List<SkinConfig>();

        [SetUp]
        public void SetUp()
        {
            _mapper = new SkinCollectionsMapper();
            _applier = new SkinCollectionsApplier();
            _registry = ScriptableObject.CreateInstance<SkinCollectionsConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var config in _createdConfigs)
            {
                Object.DestroyImmediate(config);
            }

            _createdConfigs.Clear();
            Object.DestroyImmediate(_registry);
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

            var report = _applier.Apply(new[] { config }, collections, _registry);

            Assert.That(config.Collection, Is.EqualTo(new SkinCollectionId("Collection_Phoenix")));
            Assert.That(_registry.Collections.Single().Id, Is.EqualTo(new SkinCollectionId("Collection_Phoenix")));
            Assert.That(_registry.Collections.Single().Name, Is.EqualTo("The Phoenix Collection"));
            Assert.That(report.Assigned, Is.EqualTo(new[] { "AK-47_Redline" }));
        }

        [Test]
        public void Apply_SameValue_ReportsUnchangedWithoutDirty()
        {
            var config = CreateConfig("AK-47_Redline", "Collection_Phoenix");
            var collections = new Dictionary<string, string> { { "AK-47_Redline", "The Phoenix Collection" } };

            var report = _applier.Apply(new[] { config }, collections, _registry);

            Assert.That(report.Unchanged, Is.EqualTo(new[] { "AK-47_Redline" }));
            Assert.That(report.Assigned, Is.Empty);
        }

        [TestCase("The Vertigo Collection", "Collection_Vertigo")]
        [TestCase("The 2021 Dust 2 Collection", "Collection_2021_Dust_2")]
        [TestCase("The Dreams & Nightmares Collection", "Collection_Dreams_And_Nightmares")]
        [TestCase("The St. Marc Collection", "Collection_St_Marc")]
        [TestCase("Limited Edition Item", "Collection_Limited_Edition_Item")]
        public void ToId_CollectionName_ReturnsEntityVariantId(string collectionName, string expected)
        {
            Assert.That(SkinCollectionNaming.ToId(collectionName), Is.EqualTo(new SkinCollectionId(expected)));
        }

        [Test]
        public void Apply_TwoSkinsOfOneCollection_RegistersCollectionOnce()
        {
            var first = CreateConfig("AK-47_Redline", string.Empty);
            var second = CreateConfig("AWP_Redline", string.Empty);
            var collections = new Dictionary<string, string> { { "AK-47_Redline", "The Phoenix Collection" }, { "AWP_Redline", "The Phoenix Collection" } };

            var report = _applier.Apply(new[] { first, second }, collections, _registry);

            Assert.That(_registry.Collections.Count, Is.EqualTo(1));
            Assert.That(report.Registered, Is.EqualTo(new[] { "Collection_Phoenix" }));
        }

        [Test]
        public void Apply_UnknownId_ReportsMissing()
        {
            var config = CreateConfig("AWP_Unknown", string.Empty);

            var report = _applier.Apply(new[] { config }, new Dictionary<string, string>(), _registry);

            Assert.That(report.Missing, Is.EqualTo(new[] { "AWP_Unknown" }));
            Assert.That(config.Collection.IsEmpty, Is.True);
        }

        private SkinConfig CreateConfig(string id, string collection)
        {
            var config = ScriptableObject.CreateInstance<SkinConfig>();
            config.Id = new SkinId(id);
            config.Collection = new SkinCollectionId(collection);
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

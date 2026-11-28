using System;
using System.Collections.Generic;
using System.Linq;
using Features.Cases.Configs;
using Features.RemoteConfigs.Editor.Interpreters;
using Features.Skins.Configs;
using Features.Skins.Data;
using NUnit.Framework;
using QCore.Tools.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class MissingSkinsRemovalTests
    {
        private const float MaxMissingShare = 0.1f;
        private const string SkinsFolder = "Assets/Skins";

        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var createdObject in _createdObjects)
            {
                Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
        }

        [Test]
        public void CanOffer_RemovablePageWithFewMissing_True()
        {
            var report = CreateReport(true, 95, new[] { "A", "B" });

            Assert.That(CreatePolicy().CanOffer(report), Is.True);
        }

        [Test]
        public void CanOffer_MissingShareAtLimit_True()
        {
            var report = CreateReport(true, 9, new[] { "A" });

            Assert.That(CreatePolicy().CanOffer(report), Is.True);
        }

        [Test]
        public void CanOffer_MissingShareAboveLimit_False()
        {
            var report = CreateReport(true, 1075, Enumerable.Range(0, 900).Select(index => "Missing_" + index));

            Assert.That(CreatePolicy().CanOffer(report), Is.False);
        }

        [Test]
        public void CanOffer_CreatedConfigsDoNotRaiseLimit_False()
        {
            var report = CreateReport(true, 8, new[] { "A", "B" });

            for (var index = 0; index < 100; index++)
            {
                report.Created.Add("New_" + index);
            }

            Assert.That(CreatePolicy().CanOffer(report), Is.False);
        }

        [Test]
        public void CanOffer_PageNotRemovable_False()
        {
            var report = CreateReport(false, 95, new[] { "A" });

            Assert.That(CreatePolicy().CanOffer(report), Is.False);
        }

        [Test]
        public void CanOffer_NothingMissing_False()
        {
            var report = CreateReport(true, 95, new string[0]);

            Assert.That(CreatePolicy().CanOffer(report), Is.False);
        }

        [Test]
        public void CanOffer_ImportHasErrors_False()
        {
            var report = CreateReport(true, 95, new[] { "A" });
            report.Errors.Add("broken row");

            Assert.That(CreatePolicy().CanOffer(report), Is.False);
        }

        [Test]
        public void FindRemovable_IdsReferencedByCases_Excluded()
        {
            var removable = CreatePolicy().FindRemovable(new[] { "A", "B", "C" }, new HashSet<string> { "B" });

            Assert.That(removable, Is.EqualTo(new[] { "A", "C" }));
        }

        [Test]
        public void BuildMessage_MoreIdsThanShown_ListsFirstAndRemainderCount()
        {
            var message = CreatePolicy().BuildMessage(new[] { "A", "B", "C", "D" }, new string[0], 2);
            var lines = SplitLines(message);

            Assert.That(lines, Does.Contain("A"));
            Assert.That(lines, Does.Contain("B"));
            Assert.That(lines, Does.Not.Contain("C"));
            Assert.That(message, Does.Contain("4 skin configs"));
            Assert.That(message, Does.Contain("2 more"));
        }

        [Test]
        public void BuildMessage_ReferencedIds_ListedAsKept()
        {
            var message = CreatePolicy().BuildMessage(new[] { "A" }, new[] { "B" }, 20);

            Assert.That(message, Does.Contain("Kept 1 skin configs still used by cases"));
            Assert.That(SplitLines(message), Does.Contain("B"));
        }

        [Test]
        public void Select_ConfigsWithMissingIds_ReturnsOnlyThem()
        {
            var kept = CreateSkin("CZ75-Auto_Victoria");
            var stale = CreateSkin("CZ-Auto_Victoria");
            var phantom = CreateSkin("Desert_Eagle_Sunset_Storm");

            var selected = new MissingSkinsRemover(SkinsFolder).Select(new[] { kept, stale, phantom }, new[] { "CZ-Auto_Victoria", "Desert_Eagle_Sunset_Storm", "Unknown" });

            Assert.That(selected.Select(skin => skin.Id.Value), Is.EquivalentTo(new[] { "CZ-Auto_Victoria", "Desert_Eagle_Sunset_Storm" }));
        }

        [Test]
        public void FindReferencedIds_SkinsInCases_ReturnsTheirIds()
        {
            var used = CreateSkin("AK-47_Redline");
            var other = CreateSkin("AWP_Asiimov");
            var caseConfig = ScriptableObject.CreateInstance<CaseConfig>();
            _createdObjects.Add(caseConfig);
            caseConfig.SkinsMapEntries = new WeightedMap<SkinConfig>();
            caseConfig.SkinsMapEntries.Add(used, 1f);

            var referenced = new MissingSkinsRemover(SkinsFolder).FindReferencedIds(new[] { caseConfig });

            Assert.That(referenced, Is.EquivalentTo(new[] { "AK-47_Redline" }));
            Assert.That(referenced.Contains(other.Id.Value), Is.False);
        }

        private MissingSkinsRemovalPolicy CreatePolicy()
        {
            return new MissingSkinsRemovalPolicy(MaxMissingShare);
        }

        private string[] SplitLines(string message)
        {
            return message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        private ImportReport CreateReport(bool canRemoveMissing, int updatedCount, IEnumerable<string> missing)
        {
            var report = new ImportReport
            {
                CanRemoveMissing = canRemoveMissing,
            };

            for (var index = 0; index < updatedCount; index++)
            {
                report.Updated.Add("Skin_" + index);
            }

            report.Missing.AddRange(missing);

            return report;
        }

        private SkinConfig CreateSkin(string id)
        {
            var skin = ScriptableObject.CreateInstance<SkinConfig>();
            skin.Id = new SkinId(id);
            _createdObjects.Add(skin);

            return skin;
        }
    }
}

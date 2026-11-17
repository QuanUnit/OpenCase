using System.Collections.Generic;
using System.Linq;
using Features.RemoteConfigs.Editor.Exporters.Skins;
using Features.RemoteConfigs.Editor.Models;
using NUnit.Framework;
using QCore.Tools.Mathematics;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class SkinsSheetMergerTests
    {
        private SkinsSheetMerger _merger;

        [SetUp]
        public void SetUp()
        {
            _merger = new SkinsSheetMerger();
        }

        [Test]
        public void Merge_ExistingIdInSource_ReplacedInPlace()
        {
            var existing = new List<SkinDataModel> { CreateRow("A", 1f), CreateRow("B", 1f), CreateRow("C", 1f) };
            var source = new List<SkinDataModel> { CreateRow("B", 5f) };

            var result = _merger.Merge(existing, source, new HashSet<string>());

            Assert.That(result.Rows.Select(row => row.Id), Is.EqualTo(new[] { "A", "B", "C" }));
            Assert.That(result.Rows[1].Price.From, Is.EqualTo(5f));
            Assert.That(result.UpdatedIds, Is.EqualTo(new[] { "B" }));
        }

        [Test]
        public void Merge_NewIds_AppendedInSourceOrder()
        {
            var existing = new List<SkinDataModel> { CreateRow("A", 1f) };
            var source = new List<SkinDataModel> { CreateRow("Z", 1f), CreateRow("A", 2f), CreateRow("M", 1f) };

            var result = _merger.Merge(existing, source, new HashSet<string>());

            Assert.That(result.Rows.Select(row => row.Id), Is.EqualTo(new[] { "A", "Z", "M" }));
            Assert.That(result.AddedIds, Is.EqualTo(new[] { "Z", "M" }));
        }

        [Test]
        public void Merge_ExistingIdNotInSource_KeptUnchanged()
        {
            var keptRow = CreateRow("Old", 3f);
            var existing = new List<SkinDataModel> { keptRow, CreateRow("A", 1f) };
            var source = new List<SkinDataModel> { CreateRow("A", 2f) };

            var result = _merger.Merge(existing, source, new HashSet<string>());

            Assert.That(result.Rows[0], Is.SameAs(keptRow));
            Assert.That(result.KeptIds, Is.EqualTo(new[] { "Old" }));
            Assert.That(result.Rows.Count, Is.EqualTo(2));
        }

        [Test]
        public void Merge_ExistingIdSkippedByBuilder_KeptButNotReportedAsMissing()
        {
            var existing = new List<SkinDataModel> { CreateRow("Skipped", 1f), CreateRow("Old", 1f) };

            var result = _merger.Merge(existing, new List<SkinDataModel>(), new HashSet<string> { "Skipped" });

            Assert.That(result.Rows.Select(row => row.Id), Is.EqualTo(new[] { "Skipped", "Old" }));
            Assert.That(result.KeptSkippedIds, Is.EqualTo(new[] { "Skipped" }));
            Assert.That(result.KeptIds, Is.EqualTo(new[] { "Old" }));
        }

        [Test]
        public void Merge_NewSkinOfExistingWeapon_PlacedAfterSameWeaponRows()
        {
            var existing = new List<SkinDataModel>
            {
                CreateSkinRow("AK-47", "Redline"),
                CreateSkinRow("AWP", "Asiimov"),
                CreateSkinRow("SSG_08", "Prey"),
                CreateSkinRow("AK-47", "Slate"),
            };

            var source = new List<SkinDataModel>
            {
                CreateSkinRow("AWP", "Dragon Lore"),
                CreateSkinRow("AK-47", "Wild Lotus"),
                CreateSkinRow("Zeus_x27", "Olympus"),
                CreateSkinRow("SSG_08", "Dragonfire"),
            };

            var result = _merger.Merge(existing, source, new HashSet<string>());

            var expectedIds = new[]
            {
                "AK-47_Redline", "AK-47_Slate", "AK-47_Wild_Lotus",
                "AWP_Asiimov", "AWP_Dragon_Lore",
                "SSG_08_Prey", "SSG_08_Dragonfire",
                "Zeus_x27_Olympus",
            };

            Assert.That(result.Rows.Select(row => row.Id), Is.EqualTo(expectedIds));
        }

        private SkinDataModel CreateSkinRow(string weapon, string name)
        {
            return new SkinDataModel
            {
                Id = $"{weapon}_{name}".Replace(' ', '_'),
                Name = name,
                Price = new FloatRange(1f, 1f),
            };
        }

        private SkinDataModel CreateRow(string id, float price)
        {
            return new SkinDataModel
            {
                Id = id,
                Name = id,
                Price = new FloatRange(price, price),
            };
        }
    }
}

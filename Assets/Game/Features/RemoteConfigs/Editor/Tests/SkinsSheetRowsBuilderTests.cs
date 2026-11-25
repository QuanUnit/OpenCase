using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Exporters.Skins;
using Features.Skins.Data;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class SkinsSheetRowsBuilderTests
    {
        private SkinsSheetRowsBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            var rarityMap = new Dictionary<string, Rarity>
            {
                { "Covert", Rarity.Covert },
                { "Mil-Spec Grade", Rarity.MilSpec },
                { "Contraband", Rarity.RareSpecial },
            };

            var categoryRarityMap = new Dictionary<string, Rarity>
            {
                { "Knives", Rarity.RareSpecial },
                { "Gloves", Rarity.RareSpecial },
            };

            _builder = new SkinsSheetRowsBuilder(new[] { "Agents" }, rarityMap, categoryRarityMap, "Souvenir");
        }

        [Test]
        public void Build_RegularSkin_BuildsIdNameRarityAndFlags()
        {
            var skins = new List<SourceSkin> { CreateSkin("AK-47", "Inheritance", "Rifles", "Covert", true, false, "Field-Tested") };
            var prices = new Dictionary<string, float> { { "AK-47 | Inheritance (Field-Tested)", 31.857f } };

            var result = _builder.Build(skins, prices);

            Assert.That(result.Rows.Count, Is.EqualTo(1));
            Assert.That(result.Rows[0].Id, Is.EqualTo("AK-47_Inheritance"));
            Assert.That(result.Rows[0].Name, Is.EqualTo("Inheritance"));
            Assert.That(result.Rows[0].Rarity, Is.EqualTo(Rarity.Covert));
            Assert.That(result.Rows[0].AvailableStatTrak, Is.True);
            Assert.That(result.Rows[0].AvailableSouvenir, Is.False);
        }

        [Test]
        public void Build_NameWithSpacesAndSymbols_ReplacesOnlySpaces()
        {
            var skins = new List<SourceSkin>
            {
                CreateSkin("P250", "Apep's Curse", "Pistols", "Mil-Spec Grade", false, false, "Field-Tested"),
                CreateSkin("M4A4", "龍王 (Dragon King)", "Rifles", "Covert", false, false, "Field-Tested"),
            };

            var prices = new Dictionary<string, float>
            {
                { "P250 | Apep's Curse (Field-Tested)", 1f },
                { "M4A4 | 龍王 (Dragon King) (Field-Tested)", 2f },
            };

            var result = _builder.Build(skins, prices);

            Assert.That(result.Rows[0].Id, Is.EqualTo("P250_Apep's_Curse"));
            Assert.That(result.Rows[1].Id, Is.EqualTo("M4A4_龍王_(Dragon_King)"));
        }

        [Test]
        public void Build_SeveralWears_PriceIsMinMaxOfWears()
        {
            var skins = new List<SourceSkin> { CreateSkin("AK-47", "Inheritance", "Rifles", "Covert", true, false, "Factory New", "Field-Tested", "Battle-Scarred") };

            var prices = new Dictionary<string, float>
            {
                { "AK-47 | Inheritance (Factory New)", 85.5f },
                { "AK-47 | Inheritance (Field-Tested)", 31.5f },
                { "AK-47 | Inheritance (Battle-Scarred)", 29.7f },
                { "StatTrak™ AK-47 | Inheritance (Battle-Scarred)", 10f },
                { "Souvenir AK-47 | Inheritance (Factory New)", 500f },
            };

            var result = _builder.Build(skins, prices);

            Assert.That(result.Rows[0].Price.From, Is.EqualTo(29.7f));
            Assert.That(result.Rows[0].Price.To, Is.EqualTo(85.5f));
        }

        [Test]
        public void Build_MissingWearPrice_UsesFoundWearsOnly()
        {
            var skins = new List<SourceSkin> { CreateSkin("AK-47", "Inheritance", "Rifles", "Covert", true, false, "Factory New", "Field-Tested") };
            var prices = new Dictionary<string, float> { { "AK-47 | Inheritance (Field-Tested)", 31.5f } };

            var result = _builder.Build(skins, prices);

            Assert.That(result.Rows[0].Price.From, Is.EqualTo(31.5f));
            Assert.That(result.Rows[0].Price.To, Is.EqualTo(31.5f));
        }

        [Test]
        public void Build_NoPrice_SkipsSkinAndReportsIt()
        {
            var skins = new List<SourceSkin> { CreateSkin("MP5-SD", "Lab Rats", "SMGs", "Mil-Spec Grade", false, true, "Factory New") };

            var result = _builder.Build(skins, new Dictionary<string, float>());

            Assert.That(result.Rows, Is.Empty);
            Assert.That(result.SkippedWithoutPrice, Is.EqualTo(new[] { "MP5-SD | Lab Rats" }));
            Assert.That(result.SkippedIds, Is.EquivalentTo(new[] { "MP5-SD_Lab_Rats" }));
        }

        [Test]
        public void Build_NoWears_SkipsSkin()
        {
            var skin = CreateSkin("AK-47", "Inheritance", "Rifles", "Covert", true, false);
            skin.wears = new SourceNamedEntity[0];

            var result = _builder.Build(new List<SourceSkin> { skin }, new Dictionary<string, float>());

            Assert.That(result.Rows, Is.Empty);
            Assert.That(result.SkippedWithoutPrice.Count, Is.EqualTo(1));
        }

        [Test]
        public void Build_UnknownRarity_SkipsSkinAndReportsIt()
        {
            var skins = new List<SourceSkin> { CreateSkin("AK-47", "Inheritance", "Rifles", "Extraordinary", true, false, "Field-Tested") };
            var prices = new Dictionary<string, float> { { "AK-47 | Inheritance (Field-Tested)", 1f } };

            var result = _builder.Build(skins, prices);

            Assert.That(result.Rows, Is.Empty);
            Assert.That(result.SkippedUnknownRarity, Is.EqualTo(new[] { "AK-47 | Inheritance (Extraordinary)" }));
        }

        [Test]
        public void Build_MissingRarityName_SkipsSkinWithoutThrowing()
        {
            var skin = CreateSkin("AK-47", "Inheritance", "Rifles", "Covert", true, false, "Field-Tested");
            skin.rarity = new SourceNamedEntity();

            var prices = new Dictionary<string, float> { { "AK-47 | Inheritance (Field-Tested)", 1f } };

            var result = _builder.Build(new List<SourceSkin> { skin }, prices);

            Assert.That(result.Rows, Is.Empty);
            Assert.That(result.SkippedUnknownRarity.Count, Is.EqualTo(1));
            Assert.That(result.SkippedIds, Is.EquivalentTo(new[] { "AK-47_Inheritance" }));
        }

        [Test]
        public void Build_ExcludedCategory_SkipsSilently()
        {
            var skins = new List<SourceSkin> { CreateSkin("Agent", "Sir Bloody", "Agents", "Covert", false, false, "Factory New") };
            var prices = new Dictionary<string, float> { { "Agent | Sir Bloody (Factory New)", 10f } };

            var result = _builder.Build(skins, prices);

            Assert.That(result.Rows, Is.Empty);
            Assert.That(result.SkippedWithoutPrice, Is.Empty);
            Assert.That(result.SkippedUnknownRarity, Is.Empty);
        }

        [Test]
        public void Build_Knife_UsesStarredMarketNameAndCategoryRarity()
        {
            var skins = new List<SourceSkin> { CreateStarredSkin("Karambit", "Fade", "Knives", "Covert", "Factory New", "Minimal Wear") };

            var prices = new Dictionary<string, float>
            {
                { "★ Karambit | Fade (Factory New)", 1500f },
                { "★ Karambit | Fade (Minimal Wear)", 1400f },
            };

            var result = _builder.Build(skins, prices);

            Assert.That(result.Rows.Count, Is.EqualTo(1));
            Assert.That(result.Rows[0].Id, Is.EqualTo("Karambit_Fade"));
            Assert.That(result.Rows[0].Name, Is.EqualTo("Fade"));
            Assert.That(result.Rows[0].Rarity, Is.EqualTo(Rarity.RareSpecial));
            Assert.That(result.Rows[0].Price.From, Is.EqualTo(1400f));
            Assert.That(result.Rows[0].Price.To, Is.EqualTo(1500f));
        }

        [Test]
        public void Build_VanillaKnifeWithoutWears_UsesWearlessPriceAndVanillaName()
        {
            var skin = CreateStarredSkin("Karambit", "Fade", "Knives", "Covert");
            skin.name = "★ Karambit";

            var prices = new Dictionary<string, float> { { "★ Karambit", 700f } };

            var result = _builder.Build(new List<SourceSkin> { skin }, prices);

            Assert.That(result.Rows[0].Id, Is.EqualTo("Karambit_Vanilla"));
            Assert.That(result.Rows[0].Name, Is.EqualTo("Vanilla"));
            Assert.That(result.Rows[0].Price.From, Is.EqualTo(700f));
            Assert.That(result.Rows[0].Price.To, Is.EqualTo(700f));
        }

        [Test]
        public void Build_GlovesWithUnmappedRarity_MapsToCategoryRarity()
        {
            var skins = new List<SourceSkin> { CreateStarredSkin("Sport Gloves", "Vice", "Gloves", "Extraordinary", "Field-Tested") };
            var prices = new Dictionary<string, float> { { "★ Sport Gloves | Vice (Field-Tested)", 900f } };

            var result = _builder.Build(skins, prices);

            Assert.That(result.Rows[0].Id, Is.EqualTo("Sport_Gloves_Vice"));
            Assert.That(result.Rows[0].Rarity, Is.EqualTo(Rarity.RareSpecial));
        }

        [Test]
        public void Build_Contraband_MapsToRareSpecial()
        {
            var skins = new List<SourceSkin> { CreateSkin("M4A4", "Howl", "Rifles", "Contraband", true, false, "Field-Tested") };
            var prices = new Dictionary<string, float> { { "M4A4 | Howl (Field-Tested)", 4600f } };

            var result = _builder.Build(skins, prices);

            Assert.That(result.Rows[0].Rarity, Is.EqualTo(Rarity.RareSpecial));
        }

        [Test]
        public void Build_SouvenirFlagWithSouvenirCrate_SouvenirAvailable()
        {
            var skin = CreateSkin("AWP", "Dragon Lore", "Rifles", "Covert", false, true, "Factory New");
            skin.crates = new[] { new SourceNamedEntity { name = "Cologne 2014 Cobblestone Souvenir Package" } };

            var prices = new Dictionary<string, float> { { "AWP | Dragon Lore (Factory New)", 10000f } };

            var result = _builder.Build(new List<SourceSkin> { skin }, prices);

            Assert.That(result.Rows[0].AvailableSouvenir, Is.True);
        }

        [Test]
        public void Build_SouvenirFlagWithoutSouvenirCrate_SouvenirNotAvailable()
        {
            var skin = CreateSkin("AK-47", "Inheritance", "Rifles", "Covert", true, true, "Field-Tested");
            skin.crates = new[] { new SourceNamedEntity { name = "Kilowatt Case" } };

            var prices = new Dictionary<string, float> { { "AK-47 | Inheritance (Field-Tested)", 31f } };

            var result = _builder.Build(new List<SourceSkin> { skin }, prices);

            Assert.That(result.Rows[0].AvailableSouvenir, Is.False);
        }

        [Test]
        public void Build_SameIdVariants_MergedIntoOneRow()
        {
            var souvenirVariant = CreateSkin("Glock-18", "Gamma Doppler", "Pistols", "Covert", false, true, "Battle-Scarred");
            souvenirVariant.crates = new[] { new SourceNamedEntity { name = "Souvenir Package" } };

            var skins = new List<SourceSkin>
            {
                CreateSkin("Glock-18", "Gamma Doppler", "Pistols", "Covert", false, false, "Factory New"),
                souvenirVariant,
            };

            var prices = new Dictionary<string, float>
            {
                { "Glock-18 | Gamma Doppler (Factory New)", 88f },
                { "Glock-18 | Gamma Doppler (Battle-Scarred)", 65f },
            };

            var result = _builder.Build(skins, prices);

            Assert.That(result.Rows.Count, Is.EqualTo(1));
            Assert.That(result.Rows[0].Price.From, Is.EqualTo(65f));
            Assert.That(result.Rows[0].Price.To, Is.EqualTo(88f));
            Assert.That(result.Rows[0].AvailableSouvenir, Is.True);
        }

        private SourceSkin CreateSkin(string weapon, string name, string category, string rarity, bool stattrak, bool souvenir, params string[] wears)
        {
            var wearEntities = new SourceNamedEntity[wears.Length];

            for (var index = 0; index < wears.Length; index++)
            {
                wearEntities[index] = new SourceNamedEntity { name = wears[index] };
            }

            return new SourceSkin
            {
                name = $"{weapon} | {name}",
                weapon = new SourceNamedEntity { name = weapon },
                category = new SourceNamedEntity { name = category },
                pattern = new SourceNamedEntity { name = name },
                rarity = new SourceNamedEntity { name = rarity },
                stattrak = stattrak,
                souvenir = souvenir,
                wears = wearEntities,
                crates = new SourceNamedEntity[0],
            };
        }

        private SourceSkin CreateStarredSkin(string weapon, string name, string category, string rarity, params string[] wears)
        {
            var skin = CreateSkin(weapon, name, category, rarity, true, false, wears);
            skin.name = $"★ {weapon} | {name}";

            return skin;
        }
    }
}

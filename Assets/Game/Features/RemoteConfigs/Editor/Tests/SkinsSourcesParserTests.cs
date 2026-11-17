using Features.RemoteConfigs.Editor.Exporters.Skins;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class SkinsSourcesParserTests
    {
        private SkinsSourcesParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new SkinsSourcesParser();
        }

        [Test]
        public void ParseSkins_RootArray_ParsesFields()
        {
            var json = "[{\"name\":\"AK-47 | Inheritance\",\"weapon\":{\"name\":\"AK-47\"},\"category\":{\"name\":\"Rifles\"},\"pattern\":{\"name\":\"Inheritance\"},\"rarity\":{\"name\":\"Covert\"},\"stattrak\":true,\"souvenir\":false,\"wears\":[{\"name\":\"Field-Tested\"}],\"crates\":[{\"id\":\"crate-1\",\"name\":\"Kilowatt Case\",\"image\":\"x\"}]},{\"name\":\"★ Karambit\",\"weapon\":{\"name\":\"Karambit\"},\"category\":{\"name\":\"Knives\"},\"stattrak\":true,\"souvenir\":false}]";

            var skins = _parser.ParseSkins(json);

            Assert.That(skins.Count, Is.EqualTo(2));
            Assert.That(skins[0].name, Is.EqualTo("AK-47 | Inheritance"));
            Assert.That(skins[0].weapon.name, Is.EqualTo("AK-47"));
            Assert.That(skins[0].rarity.name, Is.EqualTo("Covert"));
            Assert.That(skins[0].stattrak, Is.True);
            Assert.That(skins[0].wears[0].name, Is.EqualTo("Field-Tested"));
            Assert.That(skins[0].crates[0].name, Is.EqualTo("Kilowatt Case"));
            Assert.That(skins[1].wears, Is.Not.Null.And.Empty);
            Assert.That(skins[1].crates, Is.Not.Null.And.Empty);
            Assert.That(skins[1].rarity, Is.Not.Null);
        }

        [Test]
        public void ParsePrices_StringPrices_ParsedWithDotDecimals()
        {
            var json = "{\"success\":true,\"currency\":\"USD\",\"items\":[{\"market_hash_name\":\"AK-47 | Inheritance (Field-Tested)\",\"volume\":10,\"price\":\"31.857\"},{\"market_hash_name\":\"Broken\",\"price\":\"n/a\"}]}";

            var prices = _parser.ParsePrices(json);

            Assert.That(prices["AK-47 | Inheritance (Field-Tested)"], Is.EqualTo(31.857f));
            Assert.That(prices.ContainsKey("Broken"), Is.False);
        }

        [Test]
        public void ParsePrices_Unsuccessful_Throws()
        {
            Assert.That(() => _parser.ParsePrices("{\"success\":false}"), Throws.InvalidOperationException);
        }
    }
}

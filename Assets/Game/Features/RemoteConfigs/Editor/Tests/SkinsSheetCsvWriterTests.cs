using System.Collections.Generic;
using System.Linq;
using Features.RemoteConfigs.Editor.Exporters.Skins;
using Features.RemoteConfigs.Editor.Models;
using Features.RemoteConfigs.Editor.Parsers;
using Features.Skins.Data;
using NUnit.Framework;
using QCore.Tools.Mathematics;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class SkinsSheetCsvWriterTests
    {
        private SkinsSheetCsvWriter _writer;

        [SetUp]
        public void SetUp()
        {
            _writer = new SkinsSheetCsvWriter();
        }

        [Test]
        public void BuildValues_Row_FormatsLikeSheet()
        {
            var rows = new List<SkinDataModel> { CreateRow("CZ75-Auto_Victoria", "Victoria", Rarity.Covert, 67.22f, 101.354f, true, false) };

            var values = _writer.BuildValues(rows);

            Assert.That(values[0], Is.EqualTo(new[] { "Id", "Name", "Rarity", "Price", "AvailableStatTrak", "AvailableSouvenir" }));
            Assert.That(values[1], Is.EqualTo(new[] { "CZ75-Auto_Victoria", "Victoria", "Covert", "67,22-101,35", "TRUE", "FALSE" }));
        }

        [Test]
        public void BooleanColumns_StatTrakAndSouvenir()
        {
            Assert.That(_writer.BooleanColumns, Is.EquivalentTo(new[] { 4, 5 }));
        }

        [Test]
        public void ToCsv_PriceWithComma_IsQuoted()
        {
            var rows = new List<SkinDataModel> { CreateRow("CZ75-Auto_Victoria", "Victoria", Rarity.Covert, 67.22f, 101.35f, true, false) };

            var csv = _writer.ToCsv(_writer.BuildValues(rows));

            Assert.That(csv, Is.EqualTo("Id,Name,Rarity,Price,AvailableStatTrak,AvailableSouvenir\r\nCZ75-Auto_Victoria,Victoria,Covert,\"67,22-101,35\",TRUE,FALSE\r\n"));
        }

        [Test]
        public void ToCsv_ParsedBySkinsSheetParser_RoundTrips()
        {
            var rows = new List<SkinDataModel>
            {
                CreateRow("AK-47_Inheritance", "Inheritance", Rarity.Covert, 29.72f, 85.81f, true, false),
                CreateRow("M4A4_Howl", "Howl", Rarity.RareSpecial, 4605.89f, 7204.09f, true, false),
                CreateRow("P250_Apep's_Curse", "Apep's Curse", Rarity.MilSpec, 0.03f, 0.5f, false, true),
                CreateRow("M4A4_龍王_(Dragon_King)", "龍王 (Dragon King)", Rarity.Classified, 1f, 2f, true, true),
                CreateRow("Glock-18_\"Quoted\"", "Name, with comma", Rarity.ConsumerGrade, 0f, 0f, false, false),
            };

            var csv = _writer.ToCsv(_writer.BuildValues(rows));
            var parsedRows = new SkinsSheetParser().ParseCsv(csv).Cast<SkinDataModel>().ToList();

            Assert.That(parsedRows.Count, Is.EqualTo(rows.Count));

            for (var index = 0; index < rows.Count; index++)
            {
                Assert.That(parsedRows[index].Id, Is.EqualTo(rows[index].Id));
                Assert.That(parsedRows[index].Name, Is.EqualTo(rows[index].Name));
                Assert.That(parsedRows[index].Rarity, Is.EqualTo(rows[index].Rarity));
                Assert.That(parsedRows[index].Price.From, Is.EqualTo(rows[index].Price.From).Within(0.005f));
                Assert.That(parsedRows[index].Price.To, Is.EqualTo(rows[index].Price.To).Within(0.005f));
                Assert.That(parsedRows[index].AvailableStatTrak, Is.EqualTo(rows[index].AvailableStatTrak));
                Assert.That(parsedRows[index].AvailableSouvenir, Is.EqualTo(rows[index].AvailableSouvenir));
            }
        }

        private SkinDataModel CreateRow(string id, string name, Rarity rarity, float from, float to, bool statTrak, bool souvenir)
        {
            return new SkinDataModel
            {
                Id = id,
                Name = name,
                Rarity = rarity,
                Price = new FloatRange(from, to),
                AvailableStatTrak = statTrak,
                AvailableSouvenir = souvenir,
            };
        }
    }
}

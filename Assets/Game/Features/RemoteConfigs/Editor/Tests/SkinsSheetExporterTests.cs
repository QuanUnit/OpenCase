using System;
using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Exporters.Skins;
using NUnit.Framework;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class SkinsSheetExporterTests
    {
        private const string Header = "Id,Name,Rarity,Price,AvailableStatTrak,AvailableSouvenir\r\n";

        private const string SkinsJson =
            "[{\"name\":\"AK-47 | Inheritance\",\"weapon\":{\"name\":\"AK-47\"},\"category\":{\"name\":\"Rifles\"},\"pattern\":{\"name\":\"Inheritance\"},\"rarity\":{\"name\":\"Covert\"},\"stattrak\":true,\"souvenir\":true,\"wears\":[{\"name\":\"Field-Tested\"}],\"crates\":[{\"name\":\"Kilowatt Case\"}]}," +
            "{\"name\":\"AWP | Asiimov\",\"weapon\":{\"name\":\"AWP\"},\"category\":{\"name\":\"Rifles\"},\"pattern\":{\"name\":\"Asiimov\"},\"rarity\":{\"name\":\"Covert\"},\"stattrak\":true,\"souvenir\":true,\"wears\":[{\"name\":\"Field-Tested\"}],\"crates\":[]}," +
            "{\"name\":\"MP5-SD | Lab Rats\",\"weapon\":{\"name\":\"MP5-SD\"},\"category\":{\"name\":\"SMGs\"},\"pattern\":{\"name\":\"Lab Rats\"},\"rarity\":{\"name\":\"Restricted\"},\"stattrak\":false,\"souvenir\":true,\"wears\":[{\"name\":\"Factory New\"}],\"crates\":[]}]";

        private const string PricesJson =
            "{\"success\":true,\"items\":[{\"market_hash_name\":\"AK-47 | Inheritance (Field-Tested)\",\"price\":\"31.857\"},{\"market_hash_name\":\"AWP | Asiimov (Field-Tested)\",\"price\":\"100.5\"}]}";

        private SkinsSheetExportConfig _config;
        private SkinsSheetExporter _exporter;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<SkinsSheetExportConfig>();
            _exporter = new SkinsSheetExporter(_config);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_config);
        }

        [Test]
        public void Build_ExistingSheet_UpdatesAddsAndKeeps()
        {
            var sheetCsv = Header +
                "MP5-SD_Lab_Rats,Lab Rats,Restricted,\"0,00-0,00\",FALSE,TRUE\r\n" +
                "AK-47_Inheritance,Inheritance,Covert,\"46,92-176,99\",TRUE,FALSE\r\n" +
                "Old_Skin,Old Skin,MilSpec,\"1,00-2,00\",FALSE,FALSE\r\n";

            var result = _exporter.Build(SkinsJson, PricesJson, sheetCsv);

            Assert.That(result.MergeResult.UpdatedIds, Is.EqualTo(new[] { "AK-47_Inheritance" }));
            Assert.That(result.MergeResult.AddedIds, Is.EqualTo(new[] { "AWP_Asiimov" }));
            Assert.That(result.MergeResult.KeptIds, Is.EqualTo(new[] { "Old_Skin" }));
            Assert.That(result.MergeResult.KeptSkippedIds, Is.EqualTo(new[] { "MP5-SD_Lab_Rats" }));

            Assert.That(result.Csv, Is.EqualTo(Header +
                "MP5-SD_Lab_Rats,Lab Rats,Restricted,\"0,00-0,00\",FALSE,TRUE\r\n" +
                "AK-47_Inheritance,Inheritance,Covert,\"31,86-31,86\",TRUE,FALSE\r\n" +
                "Old_Skin,Old Skin,MilSpec,\"1,00-2,00\",FALSE,FALSE\r\n" +
                "AWP_Asiimov,Asiimov,Covert,\"100,50-100,50\",TRUE,FALSE\r\n"));
        }

        [Test]
        public void Build_EmptyIdBetweenRows_ThrowsWithRowNumber()
        {
            var existingValues = new List<List<string>>
            {
                new List<string> { "Id", "Name", "Rarity", "Price", "AvailableStatTrak", "AvailableSouvenir" },
                new List<string> { "AK-47_Inheritance", "Inheritance", "Covert", "46,92-176,99", "TRUE", "FALSE" },
                new List<string>(),
                new List<string> { "Old_Skin", "Old Skin", "MilSpec", "1,00-2,00", "FALSE", "FALSE" },
            };

            var exception = Assert.Throws<InvalidOperationException>(() => _exporter.Build(SkinsJson, PricesJson, existingValues));

            Assert.That(exception.Message, Does.Contain("sheet rows 3"));
        }

        [Test]
        public void Build_ApiValuesWithTrailingEmptyRowsAndShortRows_Merges()
        {
            var existingValues = new List<List<string>>
            {
                new List<string> { "Id", "Name", "Rarity", "Price", "AvailableStatTrak", "AvailableSouvenir" },
                new List<string> { "Old_Skin", "Old Skin", "MilSpec", "1,00-2,00", "FALSE", "FALSE" },
                new List<string> { "", "" },
            };

            var result = _exporter.Build(SkinsJson, PricesJson, existingValues);

            Assert.That(result.MergeResult.KeptIds, Is.EqualTo(new[] { "Old_Skin" }));
            Assert.That(result.MergeResult.AddedIds, Is.EqualTo(new[] { "AK-47_Inheritance", "AWP_Asiimov" }));
        }

        [Test]
        public void Build_EmptyExistingSheet_Throws()
        {
            Assert.That(() => _exporter.Build(SkinsJson, PricesJson, Header), Throws.InvalidOperationException);
        }

        [Test]
        public void Build_EmptyExistingSheetWithoutHeader_Throws()
        {
            Assert.That(() => _exporter.Build(SkinsJson, PricesJson, string.Empty), Throws.TypeOf<InvalidOperationException>());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Features.RemoteConfigs.Editor.Cases;
using Features.RemoteConfigs.Editor.Exporters;
using Features.RemoteConfigs.Editor.Models;
using Features.RemoteConfigs.Editor.Parsers;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class CasesSheetRowsBuilderTests
    {
        private CasesSheetRowsBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            _builder = new CasesSheetRowsBuilder();
        }

        [Test]
        public void BuildValues_Case_WritesCaseRowThenContinuationRows()
        {
            var cases = new List<CasesSheetCase> { CreateCase("Case_A", "Alpha", 0.39f, ("AK-47_Redline", 1f), ("P250_Sand_Dune", 26.64f)) };

            var values = _builder.BuildValues(cases);

            Assert.That(values[0], Is.EqualTo(new[] { "Id", "Name", "Price", "SkinId", "Weight" }));
            Assert.That(values[1], Is.EqualTo(new[] { "Case_A", "Alpha", "0,39", "AK-47_Redline", "1" }));
            Assert.That(values[2], Is.EqualTo(new[] { "", "", "", "P250_Sand_Dune", "26,64" }));
        }

        [Test]
        public void BuildValues_CaseWithoutSkins_WritesSingleRow()
        {
            var values = _builder.BuildValues(new List<CasesSheetCase> { CreateCase("Case_Empty", "Empty", 1f) });

            Assert.That(values.Count, Is.EqualTo(2));
            Assert.That(values[1], Is.EqualTo(new[] { "Case_Empty", "Empty", "1,00", "", "" }));
        }

        [Test]
        public void BuildValues_TinyWeight_KeepsRoundTripPrecision()
        {
            var cases = new List<CasesSheetCase> { CreateCase("Case_A", "A", 1f, ("Skin", 5e-7f), ("Other", 13.354722f)) };

            var csv = new SheetCsvFormatter().ToCsv(_builder.BuildValues(cases));
            var parsed = (CaseDataModel)new CasesSheetParser().ParseCsv(csv)[0];

            Assert.That(parsed.SkinsMap["Skin"], Is.EqualTo(5e-7f));
            Assert.That(parsed.SkinsMap["Other"], Is.EqualTo(13.354722f));
        }

        [Test]
        public void BuildValues_DuplicateIds_Throws()
        {
            var cases = new List<CasesSheetCase> { CreateCase("Case_A", "A", 1f), CreateCase("Case_A", "B", 2f) };

            Assert.Throws<InvalidOperationException>(() => _builder.BuildValues(cases));
        }

        [Test]
        public void BuildValues_ParsedByCasesSheetParser_RoundTrips()
        {
            var cases = new List<CasesSheetCase>
            {
                CreateCase("Case_A", "Alpha, the first", 0.3f, ("AK-47_Redline", 47.952f), ("M4A4_Howl", 0.13f)),
                CreateCase("Case_B", "Beta", 1234.56f, ("Glock-18_Fade", 0.000123f)),
                CreateCase("Case_C", "Gamma \"quoted\"", 0.01f, ("P90_Asiimov", 100f)),
            };

            var csv = new SheetCsvFormatter().ToCsv(_builder.BuildValues(cases));
            var parsed = new CasesSheetParser().ParseCsv(csv).Cast<CaseDataModel>().ToList();

            Assert.That(parsed.Count, Is.EqualTo(cases.Count));

            for (var index = 0; index < cases.Count; index++)
            {
                Assert.That(parsed[index].Id, Is.EqualTo(cases[index].Id));
                Assert.That(parsed[index].Name, Is.EqualTo(cases[index].Name));
                Assert.That(parsed[index].Price, Is.EqualTo(cases[index].Price).Within(1e-4f));
                Assert.That(parsed[index].SkinsMap.Keys, Is.EqualTo(cases[index].Skins.Select(skin => skin.Key)));

                foreach (var skin in cases[index].Skins)
                {
                    Assert.That(parsed[index].SkinsMap[skin.Key], Is.EqualTo(skin.Value));
                }
            }
        }

        private CasesSheetCase CreateCase(string id, string name, float price, params (string Id, float Weight)[] skins)
        {
            var skinsList = skins.Select(skin => new KeyValuePair<string, float>(skin.Id, skin.Weight)).ToList();

            return new CasesSheetCase(id, name, price, skinsList);
        }
    }
}

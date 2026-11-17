using System;
using System.Collections.Generic;
using Features.RemoteConfigs.Editor.GoogleSheets;
using NUnit.Framework;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class GoogleSheetsRequestBuilderTests
    {
        private GoogleSheetsRequestBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            _builder = new GoogleSheetsRequestBuilder();
        }

        [Test]
        public void BuildRange_PlainTitle_QuotedRange()
        {
            Assert.That(_builder.BuildRange("Skins", 1, 1454, 6), Is.EqualTo("'Skins'!A1:F1454"));
        }

        [Test]
        public void BuildRange_TitleWithQuote_DoublesQuote()
        {
            Assert.That(_builder.BuildRange("Bob's skins", 1, 2, 6), Is.EqualTo("'Bob''s skins'!A1:F2"));
        }

        [Test]
        public void BuildColumnsRange_Title_AllRowsOfColumns()
        {
            Assert.That(_builder.BuildColumnsRange("Skins", 6), Is.EqualTo("'Skins'!A:F"));
        }

        [TestCase(1, "A")]
        [TestCase(6, "F")]
        [TestCase(26, "Z")]
        [TestCase(27, "AA")]
        [TestCase(52, "AZ")]
        public void GetColumnLetters_ColumnNumber_ReturnsLetters(int columnNumber, string expected)
        {
            Assert.That(_builder.GetColumnLetters(columnNumber), Is.EqualTo(expected));
        }

        [Test]
        public void BuildValuesJson_BooleanColumns_SentAsBooleansExceptHeader()
        {
            var values = new List<List<string>>
            {
                new List<string> { "Id", "Price", "AvailableStatTrak" },
                new List<string> { "P250_Apep's_\"Curse\"", "1,00-2,00", "TRUE" },
                new List<string> { "M4A4_龍王", "3,00-4,00", "FALSE" },
            };

            var json = _builder.BuildValuesJson("'Skins'!A1:C3", values, new HashSet<int> { 2 });

            Assert.That(json, Is.EqualTo("{\"range\":\"'Skins'!A1:C3\",\"majorDimension\":\"ROWS\",\"values\":[[\"Id\",\"Price\",\"AvailableStatTrak\"],[\"P250_Apep's_\\\"Curse\\\"\",\"1,00-2,00\",true],[\"M4A4_龍王\",\"3,00-4,00\",false]]}"));
        }

        [Test]
        public void BuildValuesJson_UnknownBooleanText_Throws()
        {
            var values = new List<List<string>> { new List<string> { "Flag" }, new List<string> { "yes" } };

            Assert.That(() => _builder.BuildValuesJson("'Skins'!A1:A2", values, new HashSet<int> { 0 }), Throws.TypeOf<FormatException>());
        }

        [Test]
        public void BuildUrls_LiteralEscapedUrls()
        {
            Assert.That(_builder.BuildUpdateValuesUrl("https://api", "sheet", "'Skins'!A1:F2"), Is.EqualTo("https://api/sheet/values/%27Skins%27%21A1%3AF2?valueInputOption=RAW"));
            Assert.That(_builder.BuildClearValuesUrl("https://api", "sheet", "'Skins'!A3:F9"), Is.EqualTo("https://api/sheet/values/%27Skins%27%21A3%3AF9:clear"));
            Assert.That(_builder.BuildReadValuesUrl("https://api", "sheet", "'Skins'!A:F"), Is.EqualTo("https://api/sheet/values/%27Skins%27%21A%3AF?valueRenderOption=FORMATTED_VALUE"));
            Assert.That(_builder.BuildSheetPropertiesUrl("https://api", "sheet"), Is.EqualTo("https://api/sheet?fields=sheets.properties%28sheetId%2Ctitle%2CgridProperties%28rowCount%29%29"));
        }

        [Test]
        public void BuildAccessTokenForm_Jwt_UrlEncodedGrant()
        {
            Assert.That(_builder.BuildAccessTokenForm("a.b.c"), Is.EqualTo("grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Ajwt-bearer&assertion=a.b.c"));
        }

        [Test]
        public void FindSheet_Gid_ReturnsTitleAndRowCount()
        {
            var response = JsonUtility.FromJson<GoogleSpreadsheetResponse>("{\"sheets\":[{\"properties\":{\"sheetId\":686572383,\"title\":\"Cases\",\"gridProperties\":{\"rowCount\":100}}},{\"properties\":{\"title\":\"Skins\",\"gridProperties\":{\"rowCount\":1339}}}]}");

            var sheet = _builder.FindSheet(response, "0");

            Assert.That(sheet.title, Is.EqualTo("Skins"));
            Assert.That(sheet.gridProperties.rowCount, Is.EqualTo(1339));
            Assert.That(() => _builder.FindSheet(response, "5"), Throws.InvalidOperationException);
        }

        [Test]
        public void ParseValues_RaggedRows_ReturnsStrings()
        {
            var values = _builder.ParseValues("{\"range\":\"'Skins'!A1:F3\",\"majorDimension\":\"ROWS\",\"values\":[[\"Id\",\"Price\"],[\"AK-47_Inheritance\",\"46,92-176,99\",\"TRUE\"],[]]}");

            Assert.That(values.Count, Is.EqualTo(3));
            Assert.That(values[1], Is.EqualTo(new[] { "AK-47_Inheritance", "46,92-176,99", "TRUE" }));
            Assert.That(values[2], Is.Empty);
        }

        [Test]
        public void ParseValues_EmptySheet_ReturnsEmpty()
        {
            Assert.That(_builder.ParseValues("{\"range\":\"'Skins'!A1:F1000\",\"majorDimension\":\"ROWS\"}"), Is.Empty);
        }
    }
}

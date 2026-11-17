using System;
using System.Collections.Generic;
using NUnit.Framework;
using Shared.Tools.Formats;

namespace Shared.Tests.EditMode
{
    public class JsonReaderTests
    {
        [Test]
        public void Read_NestedValues_ParsesTypes()
        {
            var json = "{\"range\":\"'Skins'!A1:F3\",\"values\":[[\"Id\",\"Price\"],[\"P250_Apep's_\\\"Curse\\\"\",\"67,22-101,35\"],[]],\"count\":3,\"ok\":true,\"none\":null,\"text\":\"\\u9f8d\\n\"}";

            var result = (Dictionary<string, object>)new JsonReader(json).Read();
            var values = (List<object>)result["values"];

            Assert.That(result["range"], Is.EqualTo("'Skins'!A1:F3"));
            Assert.That(values.Count, Is.EqualTo(3));
            Assert.That(((List<object>)values[1])[0], Is.EqualTo("P250_Apep's_\"Curse\""));
            Assert.That(((List<object>)values[1])[1], Is.EqualTo("67,22-101,35"));
            Assert.That((List<object>)values[2], Is.Empty);
            Assert.That(result["count"], Is.EqualTo(3d));
            Assert.That(result["ok"], Is.EqualTo(true));
            Assert.That(result["none"], Is.Null);
            Assert.That(result["text"], Is.EqualTo("龍\n"));
        }

        [TestCase("{\"a\":")]
        [TestCase("[1,2")]
        [TestCase("<html>")]
        [TestCase("{\"a\":1} x")]
        public void Read_Malformed_ThrowsFormatException(string json)
        {
            Assert.That(() => new JsonReader(json).Read(), Throws.TypeOf<FormatException>());
        }
    }
}

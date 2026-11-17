using System.Text;
using NUnit.Framework;
using Shared.Tools.Formats;

namespace Shared.Tests.EditMode
{
    public class JsonStringToolsTests
    {
        [Test]
        public void AppendJsonString_SpecialCharacters_Escaped()
        {
            var builder = new StringBuilder();

            builder.AppendJsonString("a\"b\\c\nd\re\tf\u0001g'龍");

            Assert.That(builder.ToString(), Is.EqualTo("\"a\\\"b\\\\c\\nd\\re\\tf\\u0001g'龍\""));
        }
    }
}

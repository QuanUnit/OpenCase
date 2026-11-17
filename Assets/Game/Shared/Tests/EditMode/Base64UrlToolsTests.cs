using NUnit.Framework;
using Shared.Tools.Formats;

namespace Shared.Tests.EditMode
{
    public class Base64UrlToolsTests
    {
        [Test]
        public void ToBase64Url_BytesWithPaddingAndSpecialCharacters_UrlSafeWithoutPadding()
        {
            var bytes = new byte[] { 0xFB, 0xFF, 0xBF, 0x01 };

            var result = Base64UrlTools.ToBase64Url(bytes);

            Assert.That(result, Is.EqualTo("-_-_AQ"));
        }

        [Test]
        public void ToBase64Url_JwtHeader_MatchesKnownValue()
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"RS256\",\"typ\":\"JWT\"}");

            Assert.That(Base64UrlTools.ToBase64Url(bytes), Is.EqualTo("eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9"));
        }
    }
}

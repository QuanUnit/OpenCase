using System;
using System.Security.Cryptography;
using System.Text;
using Features.RemoteConfigs.Editor.GoogleSheets;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class GoogleServiceAccountJwtBuilderTests
    {
        private const int KeySize = 2048;

        private GoogleServiceAccountJwtBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            _builder = new GoogleServiceAccountJwtBuilder();
        }

        [Test]
        public void BuildClaimsJson_Values_ContainsGoogleClaims()
        {
            var claims = _builder.BuildClaimsJson("exporter@project.iam.gserviceaccount.com", "https://www.googleapis.com/auth/spreadsheets", "https://oauth2.googleapis.com/token", 1700000000, 1700003600);

            Assert.That(claims, Is.EqualTo("{\"iss\":\"exporter@project.iam.gserviceaccount.com\",\"scope\":\"https://www.googleapis.com/auth/spreadsheets\",\"aud\":\"https://oauth2.googleapis.com/token\",\"iat\":1700000000,\"exp\":1700003600}"));
        }

        [Test]
        public void BuildUnsignedToken_Claims_HeaderAndClaimsBase64Url()
        {
            var unsignedToken = _builder.BuildUnsignedToken("{\"iss\":\"a\"}");
            var parts = unsignedToken.Split('.');

            Assert.That(parts.Length, Is.EqualTo(2));
            Assert.That(parts[0], Is.EqualTo("eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9"));
            Assert.That(Encoding.UTF8.GetString(FromBase64Url(parts[1])), Is.EqualTo("{\"iss\":\"a\"}"));
        }

        [Test]
        public void Sign_ThrowawayKey_Rs256SignatureVerifies()
        {
            using var rsa = new RSACryptoServiceProvider(KeySize);
            var unsignedToken = _builder.BuildUnsignedToken("{\"iss\":\"a\"}");

            var signature = _builder.Sign(unsignedToken, rsa.ExportParameters(true));

            Assert.That(signature.Length, Is.EqualTo(KeySize / 8));
            Assert.That(rsa.VerifyData(Encoding.ASCII.GetBytes(unsignedToken), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1), Is.True);
        }

        private byte[] FromBase64Url(string value)
        {
            var base64 = value.Replace('-', '+').Replace('_', '/');
            var padding = (4 - base64.Length % 4) % 4;

            return Convert.FromBase64String(base64 + new string('=', padding));
        }
    }
}

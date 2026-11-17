using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Shared.Tools.Cryptography;

namespace Shared.Tests.EditMode
{
    public class RsaKeyToolsTests
    {
        private const int KeySize = 2048;

        private readonly byte[] _rsaAlgorithmIdentifier =
        {
            0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00,
        };

        [Test]
        public void ReadPkcs8Pem_GeneratedKey_SignatureVerifiesWithOriginalKey()
        {
            using var original = new RSACryptoServiceProvider(KeySize);
            var originalParameters = original.ExportParameters(true);
            var pem = ToPkcs8Pem(originalParameters);

            var parameters = RsaKeyTools.ReadPkcs8Pem(pem);

            using var restored = RSA.Create();
            restored.ImportParameters(parameters);

            var data = Encoding.UTF8.GetBytes("header.claims");
            var signature = restored.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            Assert.That(parameters.Modulus, Is.EqualTo(originalParameters.Modulus));
            Assert.That(original.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1), Is.True);
        }

        [Test]
        public void ReadPkcs8Pem_OpensslKeyWithShortPrivateExponent_SignatureMatchesOpenssl()
        {
            var parameters = RsaKeyTools.ReadPkcs8Pem(KnownAnswerRsaKey.Pem);

            using var rsa = RSA.Create();
            rsa.ImportParameters(parameters);

            var signature = rsa.SignData(Encoding.UTF8.GetBytes(KnownAnswerRsaKey.Message), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            Assert.That(Convert.ToBase64String(signature), Is.EqualTo(KnownAnswerRsaKey.Sha256Signature));
        }

        [Test]
        public void ReadPkcs8_NotSequence_ThrowsFormatException()
        {
            Assert.That(() => RsaKeyTools.ReadPkcs8(new byte[] { 0x02, 0x01, 0x00 }), Throws.TypeOf<FormatException>());
        }

        private string ToPkcs8Pem(RSAParameters parameters)
        {
            var rsaPrivateKey = Encode(0x30, Concat(
                Integer(new byte[] { 0 }),
                Integer(parameters.Modulus),
                Integer(parameters.Exponent),
                Integer(parameters.D),
                Integer(parameters.P),
                Integer(parameters.Q),
                Integer(parameters.DP),
                Integer(parameters.DQ),
                Integer(parameters.InverseQ)));

            var privateKeyInfo = Encode(0x30, Concat(
                Integer(new byte[] { 0 }),
                _rsaAlgorithmIdentifier,
                Encode(0x04, rsaPrivateKey)));

            var base64 = Convert.ToBase64String(privateKeyInfo, Base64FormattingOptions.InsertLineBreaks);
            return $"-----BEGIN PRIVATE KEY-----\n{base64.Replace("\r\n", "\n")}\n-----END PRIVATE KEY-----\n";
        }

        private byte[] Integer(byte[] value)
        {
            if ((value[0] & 0x80) != 0)
            {
                return Encode(0x02, Concat(new byte[] { 0 }, value));
            }

            return Encode(0x02, value);
        }

        private byte[] Encode(byte tag, byte[] content)
        {
            var result = new List<byte> { tag };

            if (content.Length < 0x80)
            {
                result.Add((byte)content.Length);
            }
            else
            {
                var lengthBytes = new List<byte>();
                var length = content.Length;

                while (length > 0)
                {
                    lengthBytes.Insert(0, (byte)(length & 0xFF));
                    length >>= 8;
                }

                result.Add((byte)(0x80 | lengthBytes.Count));
                result.AddRange(lengthBytes);
            }

            result.AddRange(content);
            return result.ToArray();
        }

        private byte[] Concat(params byte[][] parts)
        {
            var result = new List<byte>();

            foreach (var part in parts)
            {
                result.AddRange(part);
            }

            return result.ToArray();
        }
    }
}

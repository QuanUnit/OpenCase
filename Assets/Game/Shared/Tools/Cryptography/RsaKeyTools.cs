using System;
using System.Security.Cryptography;
using System.Text;

namespace Shared.Tools.Cryptography
{
    public static class RsaKeyTools
    {
        private const byte SequenceTag = 0x30;
        private const byte IntegerTag = 0x02;
        private const byte OctetStringTag = 0x04;
        private const string PemBoundaryPrefix = "-----";

        public static RSAParameters ReadPkcs8Pem(string pem)
        {
            var base64 = new StringBuilder();

            foreach (var line in pem.Split('\n'))
            {
                var trimmedLine = line.Trim();

                if (trimmedLine.Length == 0 || trimmedLine.StartsWith(PemBoundaryPrefix, StringComparison.Ordinal) == true)
                {
                    continue;
                }

                base64.Append(trimmedLine);
            }

            return ReadPkcs8(Convert.FromBase64String(base64.ToString()));
        }

        public static RSAParameters ReadPkcs8(byte[] der)
        {
            var privateKeyInfo = new DerReader(der).ReadSequence(SequenceTag);
            privateKeyInfo.ReadElement(IntegerTag);
            privateKeyInfo.ReadElement(SequenceTag);

            var privateKey = privateKeyInfo.ReadElement(OctetStringTag);
            var rsaPrivateKey = new DerReader(privateKey).ReadSequence(SequenceTag);
            rsaPrivateKey.ReadElement(IntegerTag);

            var modulus = ReadUnsignedInteger(rsaPrivateKey);
            var exponent = ReadUnsignedInteger(rsaPrivateKey);
            var privateExponent = ReadUnsignedInteger(rsaPrivateKey);
            var prime1 = ReadUnsignedInteger(rsaPrivateKey);
            var prime2 = ReadUnsignedInteger(rsaPrivateKey);
            var exponent1 = ReadUnsignedInteger(rsaPrivateKey);
            var exponent2 = ReadUnsignedInteger(rsaPrivateKey);
            var coefficient = ReadUnsignedInteger(rsaPrivateKey);

            var halfLength = (modulus.Length + 1) / 2;

            return new RSAParameters
            {
                Modulus = modulus,
                Exponent = exponent,
                D = PadLeft(privateExponent, modulus.Length),
                P = PadLeft(prime1, halfLength),
                Q = PadLeft(prime2, halfLength),
                DP = PadLeft(exponent1, halfLength),
                DQ = PadLeft(exponent2, halfLength),
                InverseQ = PadLeft(coefficient, halfLength),
            };
        }

        private static byte[] ReadUnsignedInteger(DerReader reader)
        {
            var value = reader.ReadElement(IntegerTag);
            var leadingZeros = 0;

            while (leadingZeros < value.Length - 1 && value[leadingZeros] == 0)
            {
                leadingZeros++;
            }

            var result = new byte[value.Length - leadingZeros];
            Array.Copy(value, leadingZeros, result, 0, result.Length);

            return result;
        }

        private static byte[] PadLeft(byte[] value, int length)
        {
            if (value.Length >= length)
            {
                return value;
            }

            var result = new byte[length];
            Array.Copy(value, 0, result, length - value.Length, value.Length);

            return result;
        }
    }
}

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Shared.Tools.Cryptography;
using Shared.Tools.Formats;

namespace Features.RemoteConfigs.Editor.GoogleSheets
{
    public class GoogleServiceAccountJwtBuilder
    {
        private const string HeaderJson = "{\"alg\":\"RS256\",\"typ\":\"JWT\"}";

        public string Build(GoogleServiceAccountKey key, string scope, long issuedAt, int lifetimeSeconds)
        {
            var claimsJson = BuildClaimsJson(key.client_email, scope, key.token_uri, issuedAt, issuedAt + lifetimeSeconds);
            var unsignedToken = BuildUnsignedToken(claimsJson);
            var signature = Sign(unsignedToken, RsaKeyTools.ReadPkcs8Pem(key.private_key));

            return $"{unsignedToken}.{Base64UrlTools.ToBase64Url(signature)}";
        }

        public string BuildClaimsJson(string issuer, string scope, string audience, long issuedAt, long expiresAt)
        {
            var builder = new StringBuilder();

            builder.Append("{\"iss\":").AppendJsonString(issuer);
            builder.Append(",\"scope\":").AppendJsonString(scope);
            builder.Append(",\"aud\":").AppendJsonString(audience);
            builder.Append(",\"iat\":").Append(issuedAt.ToString(CultureInfo.InvariantCulture));
            builder.Append(",\"exp\":").Append(expiresAt.ToString(CultureInfo.InvariantCulture));
            builder.Append('}');

            return builder.ToString();
        }

        public string BuildUnsignedToken(string claimsJson)
        {
            var header = Base64UrlTools.ToBase64Url(Encoding.UTF8.GetBytes(HeaderJson));
            var claims = Base64UrlTools.ToBase64Url(Encoding.UTF8.GetBytes(claimsJson));

            return $"{header}.{claims}";
        }

        public byte[] Sign(string unsignedToken, RSAParameters privateKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportParameters(privateKey);

            return rsa.SignData(Encoding.ASCII.GetBytes(unsignedToken), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}

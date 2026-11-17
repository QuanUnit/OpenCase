using System;

namespace Shared.Tools.Formats
{
    public static class Base64UrlTools
    {
        public static string ToBase64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}

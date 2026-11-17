using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Features.RemoteConfigs.Editor.Icons
{
    public static class SkinIconFileNames
    {
        public const string Extension = ".png";

        private const char Replacement = '_';
        private const char IdWordSeparator = '_';
        private const string InvalidCharacters = "\\/:*?\"<>|";

        private static readonly Regex BrowserPrefix = new Regex(@"^imgi_\d+_", RegexOptions.CultureInvariant);

        public static string ToFileName(string id)
        {
            var builder = new StringBuilder(id.Length + Extension.Length);

            foreach (var character in Normalize(id))
            {
                builder.Append(IsInvalid(character) == true ? Replacement : character);
            }

            builder.Append(Extension);
            return builder.ToString();
        }

        public static string ToFolderName(string weapon)
        {
            var builder = new StringBuilder(weapon.Length);

            foreach (var character in Normalize(weapon))
            {
                if (IsInvalid(character) == true || char.IsWhiteSpace(character) == true || character == IdWordSeparator)
                {
                    continue;
                }

                builder.Append(character);
            }

            return builder.ToString();
        }

        public static bool IsIconOf(string iconPath, string id)
        {
            var iconName = BrowserPrefix.Replace(Normalize(Path.GetFileNameWithoutExtension(iconPath)), string.Empty);
            var normalizedId = Normalize(id);
            var fileNameId = Path.GetFileNameWithoutExtension(ToFileName(id));

            return string.Equals(iconName, normalizedId, StringComparison.OrdinalIgnoreCase) == true
                   || string.Equals(iconName, fileNameId, StringComparison.OrdinalIgnoreCase) == true;
        }

        public static string NormalizePath(string path)
        {
            return Normalize(path.Replace('\\', '/'));
        }

        private static string Normalize(string value)
        {
            return value.Normalize(NormalizationForm.FormC);
        }

        private static bool IsInvalid(char character)
        {
            return char.IsControl(character) == true || InvalidCharacters.IndexOf(character) >= 0;
        }
    }
}

using System;

namespace Features.RemoteConfigs.Editor.Exporters.Skins
{
    public static class SkinIds
    {
        private const string NameSeparator = " | ";
        private const char IdSeparator = '_';

        public static string GetName(string fullName)
        {
            var separatorIndex = fullName.IndexOf(NameSeparator, StringComparison.Ordinal);

            if (separatorIndex < 0)
            {
                return fullName;
            }

            return fullName.Substring(separatorIndex + NameSeparator.Length);
        }

        public static string Create(string weaponName, string name)
        {
            return $"{weaponName}{IdSeparator}{name}".Replace(' ', IdSeparator);
        }

        public static string AddPhase(string id, string phase)
        {
            return Create(id, phase);
        }

        public static string GetWeapon(string id, string name)
        {
            var nameSuffix = IdSeparator + name.Replace(' ', IdSeparator);

            if (id.EndsWith(nameSuffix, StringComparison.Ordinal) == true && id.Length > nameSuffix.Length)
            {
                return id.Substring(0, id.Length - nameSuffix.Length);
            }

            return id;
        }

        public static string GetMarketHashName(string weaponName, string name, string wearName)
        {
            return $"{weaponName}{NameSeparator}{name} ({wearName})";
        }
    }
}

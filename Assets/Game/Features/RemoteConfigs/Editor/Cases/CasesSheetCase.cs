using System.Collections.Generic;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CasesSheetCase
    {
        public readonly string Id;
        public readonly string Name;
        public readonly float Price;
        public readonly List<KeyValuePair<string, float>> Skins;

        public CasesSheetCase(string id, string name, float price, List<KeyValuePair<string, float>> skins)
        {
            Id = id;
            Name = name;
            Price = price;
            Skins = skins;
        }
    }
}

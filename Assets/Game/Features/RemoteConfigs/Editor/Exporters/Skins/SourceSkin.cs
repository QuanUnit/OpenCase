using System;

namespace Features.RemoteConfigs.Editor.Exporters.Skins
{
    [Serializable]
    public class SourceSkin
    {
        public string name;
        public SourceNamedEntity weapon;
        public SourceNamedEntity category;
        public SourceNamedEntity pattern;
        public SourceNamedEntity rarity;
        public bool stattrak;
        public bool souvenir;
        public SourceNamedEntity[] wears;
        public SourceNamedEntity[] crates;
        public string phase;
        public string image;
    }

    [Serializable]
    public class SourceNamedEntity
    {
        public string name;
    }

    [Serializable]
    public class SourceSkinsList
    {
        public SourceSkin[] Items;
    }
}

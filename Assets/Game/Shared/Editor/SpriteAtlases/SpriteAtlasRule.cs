using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Shared.Editor.SpriteAtlases
{
    [Serializable]
    public class SpriteAtlasRule
    {
        public string SourceFolderPath => AssetDatabase.GetAssetPath(_sourceFolder);
        public string OutputFolderPath => AssetDatabase.GetAssetPath(_outputFolder);
        public bool AtlasPerSubfolder => _atlasPerSubfolder;
        public string AtlasName => _atlasName;
        public string AddressablesGroupName => _addressablesGroupName;
        public int Padding => _padding;
        public IReadOnlyList<SpriteAtlasPlatformSettings> Platforms => _platforms;

        [SerializeField] private DefaultAsset _sourceFolder;
        [SerializeField] private DefaultAsset _outputFolder;
        [SerializeField] private bool _atlasPerSubfolder;
        [SerializeField] private string _atlasName;
        [SerializeField] private string _addressablesGroupName;
        [SerializeField, Min(0)] private int _padding = 4;
        [SerializeField] private List<SpriteAtlasPlatformSettings> _platforms = new List<SpriteAtlasPlatformSettings>();
    }
}

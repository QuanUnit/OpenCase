using System;
using UnityEditor;
using UnityEngine;

namespace Shared.Editor.SpriteAtlases
{
    [Serializable]
    public class SpriteAtlasPlatformSettings
    {
        public string PlatformName => _platformName;
        public TextureImporterFormat Format => _format;
        public int CompressionQuality => _compressionQuality;
        public int MaxTextureSize => _maxTextureSize;
        public int SpriteMaxTextureSize => _spriteMaxTextureSize;

        [SerializeField] private string _platformName = "WebGL";
        [SerializeField] private TextureImporterFormat _format = TextureImporterFormat.DXT5Crunched;
        [SerializeField, Range(0, 100)] private int _compressionQuality = 80;
        [SerializeField, Min(32)] private int _maxTextureSize = 2048;
        [SerializeField, Min(32)] private int _spriteMaxTextureSize = 2048;
    }
}

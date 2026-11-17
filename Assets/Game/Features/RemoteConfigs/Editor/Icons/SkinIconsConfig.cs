using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Exporters.Skins;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Icons
{
    [CreateAssetMenu(menuName = "Configs/Editor/" + nameof(SkinIconsConfig), fileName = nameof(SkinIconsConfig))]
    public class SkinIconsConfig : ScriptableObject
    {
        public string IconsRootPath => AssetDatabase.GetAssetPath(_iconsRootFolder);
        public string SkinConfigsPath => AssetDatabase.GetAssetPath(_skinConfigsFolder);
        public string SkinsSourceUrl => _sourceConfig.SkinsSourceUrl;
        public int SourceTimeoutSeconds => _sourceConfig.RequestTimeoutSeconds;
        public int MaxParallelDownloads => _maxParallelDownloads;
        public int DownloadTimeoutSeconds => _downloadTimeoutSeconds;
        public string AddressablesGroupName => _addressablesGroupName;
        public IReadOnlyList<string> CopiedPlatforms => _copiedPlatforms;

        [Header("Folders")]
        [SerializeField] private DefaultAsset _iconsRootFolder;
        [SerializeField] private DefaultAsset _skinConfigsFolder;

        [Header("Source")]
        [SerializeField] private SkinsSheetExportConfig _sourceConfig;

        [Header("Downloads")]
        [SerializeField, Min(1)] private int _maxParallelDownloads = 8;
        [SerializeField, Min(1)] private int _downloadTimeoutSeconds = 30;

        [Header("Import")]
        [SerializeField] private string _addressablesGroupName = "Default Local Group";
        [SerializeField] private List<string> _copiedPlatforms = new List<string> { "Standalone", "iPhone", "Android", "WebGL" };
    }
}

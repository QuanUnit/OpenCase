using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Features.RemoteConfigs.Editor.Exporters.Skins;
using Features.Skins.Configs;
using QCore.AddressablesWrapper.Containers;
using Shared.Editor.SpriteAtlases;
using Shared.Tools.AssetsExtensions;
using Shared.Tools.Web;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Features.RemoteConfigs.Editor.Icons
{
    public class SkinIconsRunner
    {
        private const string TemporaryFolderName = "SkinIcons";
        private const string SpriteReferencePath = "ContextData.SpriteSource._assetReference";

        private readonly byte[] _pngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private readonly SkinIconsConfig _config;
        private readonly SkinsSourcesParser _sourcesParser = new SkinsSourcesParser();
        private readonly SkinIconsSourceMapper _sourceMapper = new SkinIconsSourceMapper();
        private readonly SkinIconsPlanner _planner = new SkinIconsPlanner();
        private readonly SkinIconWeaponResolver _weaponResolver = new SkinIconWeaponResolver();

        public SkinIconsRunner(SkinIconsConfig config)
        {
            _config = config;
        }

        public async Task<SkinIconsReport> Run(SkinIconsMode mode, bool isDryRun, Func<string, float, bool> progressChanged)
        {
            var preparation = await Prepare(mode, progressChanged);

            if (isDryRun == true)
            {
                return CreateDryRunReport(preparation);
            }

            return await Apply(preparation, progressChanged);
        }

        public async Task<SkinIconsPreparation> Prepare(SkinIconsMode mode, Func<string, float, bool> progressChanged)
        {
            var skinsJson = await DownloadTools.DownloadText(_config.SkinsSourceUrl, _config.SourceTimeoutSeconds, progress => progressChanged("Downloading skins source", progress));

            progressChanged("Building icons plan", 1f);
            return Prepare(skinsJson, mode);
        }

        public SkinIconsPreparation Prepare(string skinsJson, SkinIconsMode mode)
        {
            var sources = _sourceMapper.Map(_sourcesParser.ParseSkins(skinsJson));
            var configsByPath = LoadConfigs();
            var states = CreateStates(configsByPath);
            var foldersByWeapon = new SkinIconFolderResolver(_config.IconsRootPath).Resolve(states);

            var plan = _planner.Build(states, sources, mode, weapon => GetFolder(foldersByWeapon, weapon), File.Exists);

            return new SkinIconsPreparation(plan, states, foldersByWeapon, configsByPath);
        }

        public SkinIconsReport CreateDryRunReport(SkinIconsPreparation preparation)
        {
            var report = new SkinIconsReport(preparation.Plan, true);

            report.PlannedNewFolders.AddRange(GetMissingFolders(preparation.Plan));
            report.UnreferencedIcons.AddRange(FindUnreferencedIcons(preparation.States));

            return report;
        }

        public async Task<SkinIconsReport> Apply(SkinIconsPreparation preparation, Func<string, float, bool> progressChanged)
        {
            var report = new SkinIconsReport(preparation.Plan, false);
            report.PlannedNewFolders.AddRange(GetMissingFolders(preparation.Plan));

            var temporaryFolder = CreateTemporaryFolder();
            var failedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var downloadItems = preparation.Plan.Items.Where(item => item.Action == SkinIconAction.Download).ToList();
                var downloadedFiles = await Download(downloadItems, temporaryFolder, report, progressChanged);

                if (report.IsCancelled == true)
                {
                    return report;
                }

                progressChanged("Importing icons", 1f);
                var movedPaths = MoveFiles(downloadedFiles, report, failedPaths);

                ImportBatch(report.CreatedFolders.Concat(movedPaths), report, failedPaths);
                ApplyImporterSettings(GetExistingTargets(preparation.Plan, failedPaths), report, failedPaths);

                progressChanged("Rebuilding sprite atlases", 1f);
                RebuildAtlases(report);

                progressChanged("Assigning icons", 1f);
                Assign(preparation, movedPaths, failedPaths, report);
            }
            finally
            {
                DeleteTemporaryFolder(temporaryFolder);
            }

            report.UnreferencedIcons.AddRange(FindUnreferencedIcons(CreateStates(LoadConfigs())));

            return report;
        }

        public void Log(SkinIconsReport report)
        {
            if (report.HasProblems == true)
            {
                Debug.LogWarning(report.GetFullSummary());
            }
            else
            {
                Debug.Log(report.GetFullSummary());
            }
        }

        private Dictionary<string, SkinConfig> LoadConfigs()
        {
            var configsByPath = new Dictionary<string, SkinConfig>();
            var guids = AssetDatabase.FindAssets($"t:{nameof(SkinConfig)}", new[] { _config.SkinConfigsPath });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                configsByPath[path] = AssetDatabase.LoadAssetAtPath<SkinConfig>(path);
            }

            return configsByPath;
        }

        private List<SkinIconConfigState> CreateStates(Dictionary<string, SkinConfig> configsByPath)
        {
            var orderedConfigs = configsByPath.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToList();
            var idsWithNames = orderedConfigs
                .Select(pair => new KeyValuePair<string, string>(pair.Value.Id.Value, pair.Value.Name))
                .ToList();

            var weapons = _weaponResolver.Resolve(idsWithNames);
            var states = new List<SkinIconConfigState>(orderedConfigs.Count);

            for (var index = 0; index < orderedConfigs.Count; index++)
            {
                var config = orderedConfigs[index].Value;
                states.Add(new SkinIconConfigState(config.Id.Value, weapons[index], GetCurrentIconPath(config), orderedConfigs[index].Key));
            }

            return states;
        }

        private string GetCurrentIconPath(SkinConfig config)
        {
            var assetReference = config.ContextData.SpriteSource?.AssetReference;

            if (assetReference == null || string.IsNullOrEmpty(assetReference.AssetGUID) == true)
            {
                return null;
            }

            var path = AtlasedSpriteReferences.GetSpritePath(assetReference);

            if (string.IsNullOrEmpty(path) == true || File.Exists(path) == false)
            {
                return null;
            }

            return path;
        }

        private string GetFolder(Dictionary<string, string> foldersByWeapon, string weapon)
        {
            return foldersByWeapon.TryGetValue(weapon, out var folder) == true ? folder : null;
        }

        private List<string> GetMissingFolders(SkinIconsPlan plan)
        {
            return plan.Items
                .Select(item => GetParentFolder(item.TargetPath))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(folder => AssetDatabase.IsValidFolder(folder) == false)
                .OrderBy(folder => folder, StringComparer.Ordinal)
                .ToList();
        }

        private List<string> FindUnreferencedIcons(IReadOnlyList<SkinIconConfigState> states)
        {
            var iconPaths = AssetDatabase.FindAssets("t:Texture2D", new[] { _config.IconsRootPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct();

            var referencedPaths = states
                .Where(state => state.CurrentIconPath != null)
                .Select(state => state.CurrentIconPath);

            return _planner.FindUnreferenced(iconPaths, referencedPaths);
        }

        private string CreateTemporaryFolder()
        {
            var rootFolder = Path.Combine(Application.temporaryCachePath, TemporaryFolderName);

            DeleteTemporaryFolder(rootFolder);

            var temporaryFolder = Path.Combine(rootFolder, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporaryFolder);

            return temporaryFolder;
        }

        private void DeleteTemporaryFolder(string folder)
        {
            if (Directory.Exists(folder) == false)
            {
                return;
            }

            try
            {
                Directory.Delete(folder, true);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Skin icons temporary folder could not be deleted: {folder}: {exception.Message}");
            }
        }

        private async Task<List<DownloadedIcon>> Download(List<SkinIconsPlanItem> items, string temporaryFolder, SkinIconsReport report, Func<string, float, bool> progressChanged)
        {
            var downloadedFiles = new List<DownloadedIcon>();
            var pending = new Queue<SkinIconsPlanItem>(items);
            var active = new List<ActiveDownload>();
            var finishedCount = 0;

            try
            {
                while (pending.Count > 0 || active.Count > 0)
                {
                    while (active.Count < _config.MaxParallelDownloads && pending.Count > 0)
                    {
                        active.Add(StartDownload(pending.Dequeue()));
                    }

                    for (var index = active.Count - 1; index >= 0; index--)
                    {
                        if (active[index].Operation.isDone == false)
                        {
                            continue;
                        }

                        CompleteDownload(active[index], temporaryFolder, report, downloadedFiles);
                        active[index].Request.Dispose();
                        active.RemoveAt(index);
                        finishedCount++;
                    }

                    var isCancelled = progressChanged($"Downloading icons {finishedCount}/{items.Count}", (float)finishedCount / Mathf.Max(1, items.Count));

                    if (isCancelled == true)
                    {
                        report.IsCancelled = true;
                        return downloadedFiles;
                    }

                    await Task.Yield();
                }
            }
            finally
            {
                foreach (var download in active)
                {
                    download.Request.Abort();
                    download.Request.Dispose();
                }
            }

            return downloadedFiles;
        }

        private ActiveDownload StartDownload(SkinIconsPlanItem item)
        {
            var request = UnityWebRequest.Get(item.ImageUrl);
            request.timeout = _config.DownloadTimeoutSeconds;

            return new ActiveDownload(item, request, request.SendWebRequest());
        }

        private void CompleteDownload(ActiveDownload download, string temporaryFolder, SkinIconsReport report, List<DownloadedIcon> downloadedFiles)
        {
            var item = download.Item;

            if (download.Request.result != UnityWebRequest.Result.Success)
            {
                report.Failed.Add($"{item.Id}: {download.Request.error}");
                return;
            }

            var bytes = download.Request.downloadHandler.data;

            if (IsValidPng(bytes) == false)
            {
                report.Failed.Add($"{item.Id}: response is not a valid PNG");
                return;
            }

            var temporaryPath = Path.Combine(temporaryFolder, $"{Guid.NewGuid():N}{SkinIconFileNames.Extension}");

            try
            {
                File.WriteAllBytes(temporaryPath, bytes);
                downloadedFiles.Add(new DownloadedIcon(item, temporaryPath));
            }
            catch (Exception exception)
            {
                report.Failed.Add($"{item.Id}: temporary file could not be written: {exception.Message}");
            }
        }

        private bool IsValidPng(byte[] bytes)
        {
            if (bytes == null || bytes.Length < _pngSignature.Length)
            {
                return false;
            }

            for (var index = 0; index < _pngSignature.Length; index++)
            {
                if (bytes[index] != _pngSignature[index])
                {
                    return false;
                }
            }

            var texture = new Texture2D(2, 2);

            try
            {
                return texture.LoadImage(bytes);
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        private List<string> MoveFiles(List<DownloadedIcon> downloadedFiles, SkinIconsReport report, HashSet<string> failedPaths)
        {
            var movedPaths = new List<string>();

            foreach (var downloadedFile in downloadedFiles)
            {
                var item = downloadedFile.Item;
                var targetPath = item.TargetPath;

                try
                {
                    var folder = GetParentFolder(targetPath);

                    if (Directory.Exists(folder) == false)
                    {
                        Directory.CreateDirectory(folder);
                        report.CreatedFolders.Add(folder);
                    }

                    if (File.Exists(targetPath) == true)
                    {
                        report.Failed.Add($"{item.Id}: {targetPath} appeared during the run, not overwritten");
                        failedPaths.Add(targetPath);
                        continue;
                    }

                    File.Move(downloadedFile.TemporaryPath, targetPath);
                    movedPaths.Add(targetPath);
                    report.Downloaded.Add(item.Id);
                }
                catch (Exception exception)
                {
                    report.Failed.Add($"{item.Id}: file could not be moved to {targetPath}: {exception.Message}");
                    failedPaths.Add(targetPath);
                }
            }

            return movedPaths;
        }

        private void ImportBatch(IEnumerable<string> paths, SkinIconsReport report, HashSet<string> failedPaths)
        {
            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (var path in paths)
                {
                    try
                    {
                        AssetDatabase.ImportAsset(path);
                    }
                    catch (Exception exception)
                    {
                        report.Failed.Add($"{path}: import failed: {exception.Message}");
                        failedPaths.Add(path);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private List<string> GetExistingTargets(SkinIconsPlan plan, HashSet<string> failedPaths)
        {
            return plan.Items
                .Select(item => item.TargetPath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(path => failedPaths.Contains(path) == false && File.Exists(path) == true)
                .ToList();
        }

        private void ApplyImporterSettings(List<string> targetPaths, SkinIconsReport report, HashSet<string> failedPaths)
        {
            var excludedPaths = new HashSet<string>(targetPaths, StringComparer.OrdinalIgnoreCase);
            var referencesByFolder = new Dictionary<string, TextureImporter>(StringComparer.OrdinalIgnoreCase);

            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (var path in targetPaths)
                {
                    try
                    {
                        ApplyImporterSettings(path, excludedPaths, referencesByFolder, report, failedPaths);
                    }
                    catch (Exception exception)
                    {
                        report.Failed.Add($"{path}: importer settings failed: {exception.Message}");
                        failedPaths.Add(path);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private void ApplyImporterSettings(string path, HashSet<string> excludedPaths, Dictionary<string, TextureImporter> referencesByFolder, SkinIconsReport report, HashSet<string> failedPaths)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                report.Failed.Add($"{path}: no texture importer, the file was not imported");
                failedPaths.Add(path);
                return;
            }

            var folder = GetParentFolder(path);

            if (referencesByFolder.TryGetValue(folder, out var reference) == false)
            {
                reference = FindReferenceImporter(folder, excludedPaths);
                referencesByFolder.Add(folder, reference);
            }

            if (reference == null)
            {
                report.Errors.Add($"No reference icon to copy importer settings from, {path} imported as a default Sprite");
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
            }
            else
            {
                CopyImporterSettings(reference, importer);
            }

            importer.SaveAndReimport();
        }

        private TextureImporter FindReferenceImporter(string folder, HashSet<string> excludedPaths)
        {
            var reference = FindFirstImporter(folder, excludedPaths);

            if (reference != null)
            {
                return reference;
            }

            return FindFirstImporter(_config.IconsRootPath, excludedPaths);
        }

        private TextureImporter FindFirstImporter(string folder, HashSet<string> excludedPaths)
        {
            if (AssetDatabase.IsValidFolder(folder) == false)
            {
                return null;
            }

            var paths = AssetDatabase.FindAssets("t:Texture2D", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => excludedPaths.Contains(path) == false)
                .OrderBy(path => path, StringComparer.Ordinal);

            foreach (var path in paths)
            {
                if (AssetImporter.GetAtPath(path) is TextureImporter importer && importer.textureType == TextureImporterType.Sprite)
                {
                    return importer;
                }
            }

            return null;
        }

        private void CopyImporterSettings(TextureImporter reference, TextureImporter target)
        {
            var settings = new TextureImporterSettings();
            reference.ReadTextureSettings(settings);
            target.SetTextureSettings(settings);

            target.SetPlatformTextureSettings(reference.GetDefaultPlatformTextureSettings());

            foreach (var platform in _config.CopiedPlatforms)
            {
                var platformSettings = reference.GetPlatformTextureSettings(platform);

                if (platformSettings.overridden == true)
                {
                    target.SetPlatformTextureSettings(platformSettings);
                }
            }
        }

        private void RebuildAtlases(SkinIconsReport report)
        {
            var atlasesConfig = AssetDatabaseTools.FindAllAssets<SpriteAtlasesConfig>().First();

            report.RebuiltAtlases = SpriteAtlasesBuilder.Rebuild(atlasesConfig).Count;
        }

        private void Assign(SkinIconsPreparation preparation, List<string> movedPaths, HashSet<string> failedPaths, SkinIconsReport report)
        {
            var moved = new HashSet<string>(movedPaths, StringComparer.OrdinalIgnoreCase);
            var changedConfigs = new List<SkinConfig>();
            var atlases = AtlasedSpriteReferences.FindAllAtlases();

            try
            {
                foreach (var item in preparation.Plan.Items)
                {
                    if (item.Action == SkinIconAction.Download && moved.Contains(item.TargetPath) == false)
                    {
                        continue;
                    }

                    if (failedPaths.Contains(item.TargetPath) == true)
                    {
                        continue;
                    }

                    try
                    {
                        if (AssignItem(preparation, item, atlases, report) == true)
                        {
                            changedConfigs.Add(preparation.ConfigsByPath[item.ConfigPath]);
                        }
                    }
                    catch (Exception exception)
                    {
                        report.Failed.Add($"{item.Id}: assignment failed: {exception.Message}");
                    }
                }
            }
            finally
            {
                foreach (var config in changedConfigs)
                {
                    AssetDatabase.SaveAssetIfDirty(config);
                }
            }
        }

        private bool AssignItem(SkinIconsPreparation preparation, SkinIconsPlanItem item, List<SpriteAtlas> atlases, SkinIconsReport report)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(item.TargetPath);

            if (sprite == null)
            {
                report.Failed.Add($"{item.Id}: {item.TargetPath} is missing or not imported as a Sprite");
                return false;
            }

            var atlas = AtlasedSpriteReferences.FindAtlas(sprite, atlases);

            if (atlas == null)
            {
                report.Failed.Add($"{item.Id}: {item.TargetPath} is not packed by any sprite atlas, run Tools/Sprite Atlases/Rebuild");
                return false;
            }

            var config = preparation.ConfigsByPath[item.ConfigPath];

            config.ContextData = new SkinConfig.UIContextData
            {
                SpriteSource = new AddressableLoadContainerSprite(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(atlas))),
            };

            var serializedConfig = new SerializedObject(config);
            AtlasedSpriteReferences.Write(serializedConfig.FindProperty(SpriteReferencePath), atlas, sprite);
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(config);

            if (item.Action == SkinIconAction.ReuseExisting)
            {
                report.Reused.Add(item.Id);
            }

            report.Assigned.Add(item.Id);
            return true;
        }

        private string GetParentFolder(string path)
        {
            var separatorIndex = path.LastIndexOf('/');

            return separatorIndex < 0 ? string.Empty : path.Substring(0, separatorIndex);
        }

        private class ActiveDownload
        {
            public readonly SkinIconsPlanItem Item;
            public readonly UnityWebRequest Request;
            public readonly UnityWebRequestAsyncOperation Operation;

            public ActiveDownload(SkinIconsPlanItem item, UnityWebRequest request, UnityWebRequestAsyncOperation operation)
            {
                Item = item;
                Request = request;
                Operation = operation;
            }
        }

        private class DownloadedIcon
        {
            public readonly SkinIconsPlanItem Item;
            public readonly string TemporaryPath;

            public DownloadedIcon(SkinIconsPlanItem item, string temporaryPath)
            {
                Item = item;
                TemporaryPath = temporaryPath;
            }
        }
    }
}

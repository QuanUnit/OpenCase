using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Features.Cases.Configs;
using Features.Skins.Configs;
using QCore.AddressablesWrapper.Containers;
using QCore.Tools.Collections;
using Shared.Editor.SpriteAtlases;
using Shared.Tools.AssetsExtensions;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using Features.Cases.Data;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CaseConfigEditing
    {
        private const string CategoriesProperty = "_categoriesMap";
        private const string CategoryNameProperty = "Key.Name";
        private const string CategoryCasesProperty = "Value";
        private const string AssetNamePrefix = "CaseConfig_";
        private const string AssetExtension = ".asset";
        private const string SpriteReferencePath = "ContextData.SpriteSource._assetReference";

        public IReadOnlyList<KeyValue<CasesCategory, List<CaseConfig>>> Categories => _config.AllCasesConfig.CategoriesMap;

        private readonly CaseEditorConfig _config;

        private List<CaseConfig> _allCases;

        public CaseConfigEditing(CaseEditorConfig config)
        {
            _config = config;
        }

        public void InvalidateCases()
        {
            _allCases = null;
        }

        public List<CaseConfig> GetAllCases()
        {
            if (_allCases == null || _allCases.Any(caseConfig => caseConfig == null) == true)
            {
                _allCases = AssetDatabaseTools.FindAllAssets<CaseConfig>()
                    .OrderBy(caseConfig => caseConfig.Id.Value, StringComparer.Ordinal)
                    .ToList();
            }

            return _allCases;
        }

        public List<CaseConfig> GetCasesWithoutCategory()
        {
            var categorized = new HashSet<CaseConfig>(Categories.SelectMany(category => category.Value).Where(caseConfig => caseConfig != null));

            return GetAllCases().Where(caseConfig => categorized.Contains(caseConfig) == false).ToList();
        }

        public int FindCategoryIndex(CaseConfig caseConfig)
        {
            for (var index = 0; index < Categories.Count; index++)
            {
                if (Categories[index].Value.Contains(caseConfig) == true)
                {
                    return index;
                }
            }

            return -1;
        }

        public List<CaseConfig> GetCategorizedCasesInOrder()
        {
            return Categories.SelectMany(category => category.Value).Where(caseConfig => caseConfig != null).Distinct().ToList();
        }

        public void AddCategory(string name)
        {
            var categories = GetCategoriesObject();
            var property = categories.FindProperty(CategoriesProperty);

            property.arraySize++;

            var element = property.GetArrayElementAtIndex(property.arraySize - 1);
            element.FindPropertyRelative(CategoryNameProperty).stringValue = name;
            element.FindPropertyRelative(CategoryCasesProperty).arraySize = 0;

            ApplyCategories(categories, "Add case category");
        }

        public void RenameCategory(int categoryIndex, string name)
        {
            var categories = GetCategoriesObject();
            var element = categories.FindProperty(CategoriesProperty).GetArrayElementAtIndex(categoryIndex);

            element.FindPropertyRelative(CategoryNameProperty).stringValue = name;

            ApplyCategories(categories, "Rename case category");
        }

        public void RemoveEmptyCategory(int categoryIndex)
        {
            if (Categories[categoryIndex].Value.Count > 0)
            {
                throw new InvalidOperationException("Only an empty category can be removed");
            }

            var categories = GetCategoriesObject();
            categories.FindProperty(CategoriesProperty).DeleteArrayElementAtIndex(categoryIndex);

            ApplyCategories(categories, "Remove case category");
        }

        public void RemoveMissingCases(int categoryIndex)
        {
            var categories = GetCategoriesObject();
            var casesProperty = categories.FindProperty(CategoriesProperty).GetArrayElementAtIndex(categoryIndex).FindPropertyRelative(CategoryCasesProperty);

            for (var caseIndex = casesProperty.arraySize - 1; caseIndex >= 0; caseIndex--)
            {
                if (casesProperty.GetArrayElementAtIndex(caseIndex).objectReferenceValue == null)
                {
                    casesProperty.DeleteArrayElementAtIndex(caseIndex);
                }
            }

            ApplyCategories(categories, "Remove missing cases");
        }

        public void MoveToCategory(CaseConfig caseConfig, int categoryIndex)
        {
            MoveToCategory(caseConfig, categoryIndex, -1);
        }

        public void RemoveFromCategory(CaseConfig caseConfig)
        {
            MoveToCategory(caseConfig, -1, -1);
        }

        public CaseConfig CreateCase(int categoryIndex)
        {
            var caseConfig = ScriptableObject.CreateInstance<CaseConfig>();
            caseConfig.Id = new CaseId(GetFreeId(_config.NewCaseIdPrefix, 1));
            caseConfig.Name = _config.NewCaseName;
            caseConfig.Price = _config.NewCasePrice;
            caseConfig.SkinsMapEntries = new WeightedMap<SkinConfig>();
            AssignIcon(caseConfig, _config.DefaultCaseIcon);

            CreateAsset(caseConfig);

            if (categoryIndex >= 0)
            {
                MoveToCategory(caseConfig, categoryIndex, -1);
            }

            return caseConfig;
        }

        public CaseConfig DuplicateCase(CaseConfig source)
        {
            var caseConfig = UnityEngine.Object.Instantiate(source);
            caseConfig.Id = new CaseId(GetFreeId(source.Id.Value + _config.DuplicateIdSuffix, 0));

            CreateAsset(caseConfig);

            var categoryIndex = FindCategoryIndex(source);

            if (categoryIndex >= 0)
            {
                var sourceIndex = Categories[categoryIndex].Value.IndexOf(source);
                MoveToCategory(caseConfig, categoryIndex, sourceIndex + 1);
            }

            return caseConfig;
        }

        public string ValidateId(CaseConfig caseConfig, string id)
        {
            if (string.IsNullOrWhiteSpace(id) == true)
            {
                return "Id must not be empty";
            }

            if (id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || id.Contains(",") == true)
            {
                return "Id contains characters not allowed in a file name or a sheet cell";
            }

            if (GetAllCases().Any(other => other != caseConfig && other.Id.Value == id) == true)
            {
                return $"Id {id} is already used by another case";
            }

            return null;
        }

        public void SetId(CaseConfig caseConfig, string id)
        {
            Undo.RecordObject(caseConfig, "Change case id");
            caseConfig.Id = new CaseId(id);
            Save(caseConfig);

            var path = AssetDatabase.GetAssetPath(caseConfig);
            var error = AssetDatabase.RenameAsset(path, AssetNamePrefix + id);

            if (string.IsNullOrEmpty(error) == false)
            {
                Debug.LogWarning($"Case id changed but the asset was not renamed: {error}");
            }
        }

        public void SetName(CaseConfig caseConfig, string name)
        {
            Undo.RecordObject(caseConfig, "Change case name");
            caseConfig.Name = name;
            Save(caseConfig);
        }

        public void SetPrice(CaseConfig caseConfig, float price)
        {
            Undo.RecordObject(caseConfig, "Change case price");
            caseConfig.Price = price;
            Save(caseConfig);
        }

        public void SetIcon(CaseConfig caseConfig, Sprite sprite)
        {
            if (sprite == null)
            {
                throw new InvalidOperationException("A case must have an icon, the game cannot load a case without one");
            }

            Undo.RecordObject(caseConfig, "Change case icon");
            AssignIcon(caseConfig, sprite);
            Save(caseConfig);
        }

        public Sprite GetIcon(CaseConfig caseConfig)
        {
            var spriteSource = caseConfig.ContextData.SpriteSource;

            if (spriteSource == null || spriteSource.AssetReference == null || string.IsNullOrEmpty(spriteSource.AssetReference.AssetGUID) == true)
            {
                return null;
            }

            return AtlasedSpriteReferences.LoadSprite(spriteSource.AssetReference);
        }

        public bool IsIconAddressable(CaseConfig caseConfig)
        {
            var spriteSource = caseConfig.ContextData.SpriteSource;

            if (spriteSource == null || spriteSource.AssetReference == null || string.IsNullOrEmpty(spriteSource.AssetReference.AssetGUID) == true)
            {
                return false;
            }

            if (string.IsNullOrEmpty(spriteSource.AssetReference.SubObjectName) == true)
            {
                return false;
            }

            return AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(spriteSource.AssetReference.AssetGUID) != null;
        }

        public List<KeyValue<SkinConfig, float>> GetEntries(CaseConfig caseConfig)
        {
            if (caseConfig.SkinsMapEntries == null)
            {
                return new List<KeyValue<SkinConfig, float>>();
            }

            return caseConfig.SkinsMapEntries.Entries.ToList();
        }

        public void SetEntries(CaseConfig caseConfig, IReadOnlyList<KeyValue<SkinConfig, float>> entries, string undoName)
        {
            var map = new WeightedMap<SkinConfig>();

            foreach (var entry in entries)
            {
                map.Add(entry.Key, entry.Value);
            }

            Undo.RecordObject(caseConfig, undoName);
            caseConfig.SkinsMapEntries = map;
            Save(caseConfig);
        }

        public void SaveAllDirty()
        {
            AssetDatabase.SaveAssetIfDirty(_config.AllCasesConfig);

            foreach (var caseConfig in GetAllCases())
            {
                AssetDatabase.SaveAssetIfDirty(caseConfig);
            }
        }

        private void MoveToCategory(CaseConfig caseConfig, int categoryIndex, int position)
        {
            var categories = GetCategoriesObject();
            var categoriesProperty = categories.FindProperty(CategoriesProperty);

            for (var index = 0; index < categoriesProperty.arraySize; index++)
            {
                var casesProperty = categoriesProperty.GetArrayElementAtIndex(index).FindPropertyRelative(CategoryCasesProperty);

                for (var caseIndex = casesProperty.arraySize - 1; caseIndex >= 0; caseIndex--)
                {
                    var element = casesProperty.GetArrayElementAtIndex(caseIndex);

                    if (element.objectReferenceValue == caseConfig)
                    {
                        element.objectReferenceValue = null;
                        casesProperty.DeleteArrayElementAtIndex(caseIndex);
                    }
                }
            }

            if (categoryIndex >= 0)
            {
                var targetCases = categoriesProperty.GetArrayElementAtIndex(categoryIndex).FindPropertyRelative(CategoryCasesProperty);
                var insertIndex = position < 0 || position > targetCases.arraySize ? targetCases.arraySize : position;

                targetCases.InsertArrayElementAtIndex(insertIndex);
                targetCases.GetArrayElementAtIndex(insertIndex).objectReferenceValue = caseConfig;
            }

            ApplyCategories(categories, "Change case category");
        }

        private SerializedObject GetCategoriesObject()
        {
            return new SerializedObject(_config.AllCasesConfig);
        }

        private void ApplyCategories(SerializedObject categories, string undoName)
        {
            Undo.SetCurrentGroupName(undoName);
            categories.ApplyModifiedProperties();
            AssetDatabase.SaveAssetIfDirty(_config.AllCasesConfig);
        }

        private void CreateAsset(CaseConfig caseConfig)
        {
            var path = $"{_config.CasesFolderPath}/{AssetNamePrefix}{caseConfig.Id.Value}{AssetExtension}";

            AssetDatabase.CreateAsset(caseConfig, path);
            AssetDatabase.SaveAssetIfDirty(caseConfig);
            InvalidateCases();
        }

        private string GetFreeId(string baseId, int firstNumber)
        {
            var usedIds = new HashSet<string>(GetAllCases().Select(caseConfig => caseConfig.Id.Value), StringComparer.Ordinal);

            if (firstNumber == 0 && IsFree(baseId, usedIds) == true)
            {
                return baseId;
            }

            var number = Math.Max(1, firstNumber);

            while (IsFree(baseId + number, usedIds) == false)
            {
                number++;
            }

            return baseId + number;
        }

        private bool IsFree(string id, HashSet<string> usedIds)
        {
            var path = $"{_config.CasesFolderPath}/{AssetNamePrefix}{id}{AssetExtension}";

            return usedIds.Contains(id) == false && File.Exists(path) == false;
        }

        private void AssignIcon(CaseConfig caseConfig, Sprite sprite)
        {
            var atlas = AtlasedSpriteReferences.FindAtlas(sprite, AtlasedSpriteReferences.FindAllAtlases());

            if (atlas == null)
            {
                throw new InvalidOperationException($"Icon '{sprite.name}' is not packed by any sprite atlas: put it into the cases icons folder and run Tools/Sprite Atlases/Rebuild");
            }

            var atlasGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(atlas));
            caseConfig.ContextData = new CaseConfig.UIContextData { SpriteSource = new AddressableLoadContainerSprite(atlasGuid) };

            var serializedConfig = new SerializedObject(caseConfig);
            AtlasedSpriteReferences.Write(serializedConfig.FindProperty(SpriteReferencePath), atlas, sprite);
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
        }

        private void Save(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
        }
    }
}

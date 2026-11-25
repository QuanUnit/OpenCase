using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

namespace Shared.Editor.SpriteAtlases
{
    public static class AtlasedSpriteReferences
    {
        private const string AssetGuidField = "m_AssetGUID";
        private const string SubObjectNameField = "m_SubObjectName";
        private const string SubObjectGuidField = "m_SubObjectGUID";
        private const string SubObjectTypeField = "m_SubObjectType";

        public static List<SpriteAtlas> FindAllAtlases()
        {
            return AssetDatabase.FindAssets("t:" + nameof(SpriteAtlas))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SpriteAtlas>)
                .ToList();
        }

        public static SpriteAtlas FindAtlas(Sprite sprite, IEnumerable<SpriteAtlas> atlases)
        {
            return atlases.FirstOrDefault(atlas => atlas.CanBindTo(sprite) == true);
        }

        public static void Write(SerializedProperty assetReference, SpriteAtlas atlas, Sprite sprite)
        {
            var atlasGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(atlas));
            var spriteGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sprite));

            assetReference.FindPropertyRelative(AssetGuidField).stringValue = atlasGuid;
            assetReference.FindPropertyRelative(SubObjectNameField).stringValue = sprite.name;
            assetReference.FindPropertyRelative(SubObjectGuidField).stringValue = spriteGuid;
            assetReference.FindPropertyRelative(SubObjectTypeField).stringValue = typeof(Sprite).AssemblyQualifiedName;
        }

        public static string GetSpriteGuid(AssetReference assetReference)
        {
            if (string.IsNullOrEmpty(assetReference.SubObjectGUID) == false)
            {
                return assetReference.SubObjectGUID;
            }

            return assetReference.AssetGUID;
        }

        public static string GetSpritePath(AssetReference assetReference)
        {
            return AssetDatabase.GUIDToAssetPath(GetSpriteGuid(assetReference));
        }

        public static Sprite LoadSprite(AssetReference assetReference)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(GetSpritePath(assetReference));
        }
    }
}

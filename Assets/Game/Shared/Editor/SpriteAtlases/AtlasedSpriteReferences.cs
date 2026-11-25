using System;
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

        public static AssetReferenceAtlasedSprite Create(SpriteAtlas atlas, Sprite sprite)
        {
            var atlasGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(atlas));
            var assetReference = new AssetReferenceAtlasedSprite(atlasGuid);

            if (assetReference.SetEditorSubObject(sprite) == false)
            {
                throw new InvalidOperationException($"Sprite '{sprite.name}' is not packed by atlas '{atlas.name}'");
            }

            return assetReference;
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

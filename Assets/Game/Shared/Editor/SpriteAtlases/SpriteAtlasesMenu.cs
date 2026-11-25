using System.Linq;
using Shared.Tools.AssetsExtensions;
using UnityEditor;
using UnityEngine;

namespace Shared.Editor.SpriteAtlases
{
    public static class SpriteAtlasesMenu
    {
        private const string ProgressTitle = "Sprite Atlases";

        [MenuItem("Tools/Sprite Atlases/Rebuild")]
        public static void Rebuild()
        {
            try
            {
                EditorUtility.DisplayProgressBar(ProgressTitle, "Rebuilding atlases", 0f);

                var config = AssetDatabaseTools.FindAllAssets<SpriteAtlasesConfig>().First();
                var atlases = SpriteAtlasesBuilder.Rebuild(config);

                Debug.Log($"Sprite atlases: rebuilt {atlases.Count}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}

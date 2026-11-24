using System;
using Features.Skins.Data;
using Shared.Identifiers;

namespace Features.RemoteConfigs.Editor.Collections
{
    public static class SkinCollectionNaming
    {
        private const string ArticlePrefix = "The ";
        private const string CollectionSuffix = " Collection";

        public static SkinCollectionId ToId(string collectionName)
        {
            var variant = collectionName.Trim();

            if (variant.StartsWith(ArticlePrefix, StringComparison.Ordinal) == true)
            {
                variant = variant.Substring(ArticlePrefix.Length);
            }

            if (variant.EndsWith(CollectionSuffix, StringComparison.Ordinal) == true)
            {
                variant = variant.Substring(0, variant.Length - CollectionSuffix.Length);
            }

            return EntityIds.Compose<SkinCollectionId>(SkinCollectionId.EntityName, variant);
        }
    }
}

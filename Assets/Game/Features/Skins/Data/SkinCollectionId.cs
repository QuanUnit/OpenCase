using System;
using Shared.Identifiers;

namespace Features.Skins.Data
{
    [Serializable]
    public sealed class SkinCollectionId : EntityId
    {
        public const string EntityName = "Collection";

        public SkinCollectionId()
        {
        }

        public SkinCollectionId(string value) : base(value)
        {
        }
    }
}

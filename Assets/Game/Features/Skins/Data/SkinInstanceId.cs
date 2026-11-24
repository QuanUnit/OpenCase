using System;
using Shared.Identifiers;

namespace Features.Skins.Data
{
    [Serializable]
    public sealed class SkinInstanceId : EntityId
    {
        public const string EntityName = "SkinInstance";

        public SkinInstanceId()
        {
        }

        public SkinInstanceId(string value) : base(value)
        {
        }
    }
}

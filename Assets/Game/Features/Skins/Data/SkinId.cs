using System;
using Shared.Identifiers;

namespace Features.Skins.Data
{
    [Serializable]
    public sealed class SkinId : EntityId
    {
        public SkinId()
        {
        }

        public SkinId(string value) : base(value)
        {
        }
    }
}

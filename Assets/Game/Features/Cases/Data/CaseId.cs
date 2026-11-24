using System;
using Shared.Identifiers;

namespace Features.Cases.Data
{
    [Serializable]
    public sealed class CaseId : EntityId
    {
        public const string EntityName = "Case";

        public CaseId()
        {
        }

        public CaseId(string value) : base(value)
        {
        }
    }
}

using System;
using UnityEngine;

namespace Shared.Identifiers
{
    [Serializable]
    public abstract class EntityId : IEquatable<EntityId>
    {
        [SerializeField] private string _value;

        public string Value => _value ?? string.Empty;
        public bool IsEmpty => string.IsNullOrEmpty(_value);

        protected EntityId()
        {
            _value = string.Empty;
        }

        protected EntityId(string value)
        {
            _value = value ?? string.Empty;
        }

        public bool Equals(EntityId other)
        {
            if (ReferenceEquals(other, null) == true)
            {
                return false;
            }

            if (ReferenceEquals(this, other) == true)
            {
                return true;
            }

            return GetType() == other.GetType() && string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EntityId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GetType(), StringComparer.Ordinal.GetHashCode(Value));
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(EntityId left, EntityId right)
        {
            if (ReferenceEquals(left, null) == true)
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(EntityId left, EntityId right)
        {
            return (left == right) == false;
        }
    }
}

using System;
using AdvancedLayoutGroups.Tools.Extensions;
using UnityEngine;

namespace AdvancedLayoutGroups.Implementation.Data
{
    [Serializable]
    public struct LayoutElementsPlacementAnimationSettings
    {
        public AnimationCurve Curve;
        public float Duration;
        
        public bool Equals(LayoutElementsPlacementAnimationSettings other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is LayoutElementsPlacementAnimationSettings other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Curve, Duration);
        }

        public static bool operator ==(LayoutElementsPlacementAnimationSettings a,
            LayoutElementsPlacementAnimationSettings b)
        {
            return a.Curve.ValuesEquals(b.Curve) && a.Duration == b.Duration;
        }

        public static bool operator !=(LayoutElementsPlacementAnimationSettings a, LayoutElementsPlacementAnimationSettings b)
        {
            return !(a == b);
        }
    }
}
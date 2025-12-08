using UnityEngine;

namespace AdvancedLayoutGroups.Tools.Extensions
{
    public static class AnimationCurveExtensions
    {
        public static bool ValuesEquals(this AnimationCurve source, AnimationCurve another)
        {
            if (source == null || another == null) return false;
            
            Keyframe[] keysA = source.keys;
            Keyframe[] keysB = another.keys;
            
            if (keysA == null || keysB == null) return false;

            if (keysA.Length != keysB.Length) return false;

            for (int i = 0; i < keysA.Length; i++)
            {
                Keyframe keyA = keysA[i];
                Keyframe keyB = keysB[i];

                if (keyA.Equals(keyB) == false) return false;
            }

            return true;
        }
    }
}
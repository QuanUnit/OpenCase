using System;

namespace Tools.Mathematics
{
    [Serializable]
    public struct IntRange
    {
        public int Min;
        public int Max;

        public bool IsInside(int value)
        {
            return value >= Min && value <= Max;
        }

        public int Random()
        {
            return UnityEngine.Random.Range(Min, Max + 1);
        }
    }
}
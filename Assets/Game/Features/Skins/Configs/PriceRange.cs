using System;
using UnityEngine;

namespace Features.Skins.Configs
{
    [Serializable]
    public struct PriceRange
    {
        public float MinPrice;
        public float MaxPrice;

        public PriceRange(float min, float max)
        {
            MinPrice = min;
            MaxPrice = max;
        }
        
        public float GetInRange(float t)
        {
            return Mathf.Lerp(MinPrice, MaxPrice, t);
        }
    }
}
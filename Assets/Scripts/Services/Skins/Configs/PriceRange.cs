using System;
using UnityEngine;

namespace Services.Skins.Configs
{
    [Serializable]
    public struct PriceRange
    {
        public float MinPrice;
        public float MaxPrice;

        public float GetInRange(float t)
        {
            return Mathf.Lerp(MinPrice, MaxPrice, t);
        }
    }
}
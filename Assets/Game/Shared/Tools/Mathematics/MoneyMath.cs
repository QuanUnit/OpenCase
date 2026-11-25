using Shared.UI.Effects;
using UnityEngine;

namespace Shared.Tools.Mathematics
{
    public static class MoneyMath
    {
        public static float RoundToMoney(float value)
        {
            var scale = GetMinorUnitsScale();

            return Mathf.Round(value * scale) / scale;
        }

        public static float FloorToMoney(float value)
        {
            var scale = GetMinorUnitsScale();

            return Mathf.Floor(value * scale) / scale;
        }

        public static int ToMinorUnits(float value)
        {
            return Mathf.RoundToInt(value * GetMinorUnitsScale());
        }

        private static float GetMinorUnitsScale()
        {
            return Mathf.Pow(10f, NumberCounterFormatter.MoneyDecimals);
        }
    }
}

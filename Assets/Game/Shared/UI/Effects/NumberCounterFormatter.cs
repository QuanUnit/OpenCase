using System;
using System.Globalization;

namespace Shared.UI.Effects
{
    public static class NumberCounterFormatter
    {
        public const int MoneyDecimals = 2;
        public const string MoneyPostfix = " $";

        public static string FormatMoney(float value)
        {
            return Format(value, MoneyDecimals, "", MoneyPostfix);
        }

        public static string Format(float value, int decimals, string prefix, string postfix)
        {
            var format = "F" + decimals;
            var number = value.ToString(format, CultureInfo.InvariantCulture);

            if (number.StartsWith("-", StringComparison.Ordinal) == true && double.Parse(number, CultureInfo.InvariantCulture) == 0d)
            {
                number = number.Substring(1);
            }

            return $"{prefix}{number}{postfix}";
        }

        public static int GetChangeDirection(float previous, float current, float tolerance)
        {
            var delta = current - previous;

            if (delta > tolerance)
            {
                return 1;
            }

            if (delta < -tolerance)
            {
                return -1;
            }

            return 0;
        }
    }
}

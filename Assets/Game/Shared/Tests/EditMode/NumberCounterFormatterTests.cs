using NUnit.Framework;
using Shared.UI.Effects;

namespace Shared.Tests.EditMode
{
    public class NumberCounterFormatterTests
    {
        [TestCase(1234.5f, 2, "", " $", "1234.50 $")]
        [TestCase(0f, 2, "", " $", "0.00 $")]
        [TestCase(7.6f, 0, "$", "", "$8")]
        [TestCase(-3.24f, 1, "", "", "-3.2")]
        [TestCase(-0.001f, 2, "", " $", "0.00 $")]
        [TestCase(-0.4f, 0, "", "", "0")]
        public void Format_ValueWithDecimals_ReturnsExpected(float value, int decimals, string prefix, string postfix, string expected)
        {
            var result = NumberCounterFormatter.Format(value, decimals, prefix, postfix);

            Assert.AreEqual(expected, result);
        }

        [TestCase(1234.5f, "1234.50 $")]
        [TestCase(-0.004f, "0.00 $")]
        [TestCase(-12.5f, "-12.50 $")]
        public void FormatMoney_Value_UsesTwoDecimalsAndDollarPostfix(float value, string expected)
        {
            var result = NumberCounterFormatter.FormatMoney(value);

            Assert.AreEqual(expected, result);
        }

        [TestCase(10f, 15f, 1)]
        [TestCase(15f, 10f, -1)]
        [TestCase(10f, 10f, 0)]
        [TestCase(10f, 10.00001f, 0)]
        public void GetChangeDirection_Values_ReturnsSign(float previous, float current, int expected)
        {
            var result = NumberCounterFormatter.GetChangeDirection(previous, current, 0.0001f);

            Assert.AreEqual(expected, result);
        }
    }
}

using NUnit.Framework;
using Shared.Tools.Drawing;
using UnityEngine;

namespace Shared.Tests.EditMode
{
    public class InkCanvasTests
    {
        [Test]
        public void DrawDot_Center_InksCircleArea()
        {
            var canvas = new InkCanvas(20, 20);

            canvas.DrawDot(new Vector2(10f, 10f), 3f);

            Assert.IsTrue(canvas.IsInked(10, 10) == true);
            Assert.IsTrue(canvas.IsInked(0, 0) == false);
            Assert.AreEqual(Mathf.PI * 9f, canvas.InkedPixelsCount, 6f);
        }

        [Test]
        public void DrawLine_Horizontal_InksContinuousStroke()
        {
            var canvas = new InkCanvas(20, 20);

            canvas.DrawLine(new Vector2(2f, 10f), new Vector2(17f, 10f), 1f);

            for (var x = 2; x <= 17; x++)
            {
                Assert.IsTrue(canvas.IsInked(x, 10) == true, "x=" + x);
            }
        }

        [Test]
        public void DrawLine_OutsideBounds_ClipsWithoutException()
        {
            var canvas = new InkCanvas(20, 20);

            Assert.DoesNotThrow(() => canvas.DrawLine(new Vector2(-30f, -30f), new Vector2(50f, 50f), 4f));
            Assert.IsTrue(canvas.InkedPixelsCount > 0);
        }

        [Test]
        public void Clear_ResetsCount()
        {
            var canvas = new InkCanvas(20, 20);
            canvas.DrawDot(new Vector2(10f, 10f), 3f);

            canvas.Clear();

            Assert.AreEqual(0, canvas.InkedPixelsCount);
            Assert.IsTrue(canvas.IsInked(10, 10) == false);
        }

        [Test]
        public void Coverage_IsInkedRatio()
        {
            var canvas = new InkCanvas(20, 20);

            canvas.DrawDot(new Vector2(10f, 10f), 3f);

            Assert.AreEqual(canvas.InkedPixelsCount / 400f, canvas.Coverage, 0.0001f);
        }
    }
}

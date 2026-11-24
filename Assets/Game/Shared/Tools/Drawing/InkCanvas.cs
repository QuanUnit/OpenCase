using UnityEngine;

namespace Shared.Tools.Drawing
{
    public class InkCanvas
    {
        private const float StepFactor = 0.5f;
        private const float PixelCenterOffset = 0.5f;
        private const float MinimumStep = 0.01f;

        private readonly bool[] _pixels;

        public InkCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            _pixels = new bool[width * height];
        }

        public int Width { get; }
        public int Height { get; }
        public int InkedPixelsCount { get; private set; }
        public float Coverage => (float)InkedPixelsCount / (Width * Height);

        public bool IsInked(int x, int y)
        {
            return _pixels[y * Width + x];
        }

        public void DrawDot(Vector2 center, float radius)
        {
            var minX = Mathf.Max(0, Mathf.FloorToInt(center.x - radius));
            var maxX = Mathf.Min(Width - 1, Mathf.CeilToInt(center.x + radius));
            var minY = Mathf.Max(0, Mathf.FloorToInt(center.y - radius));
            var maxY = Mathf.Min(Height - 1, Mathf.CeilToInt(center.y + radius));
            var squaredRadius = radius * radius;

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var offsetX = x + PixelCenterOffset - center.x;
                    var offsetY = y + PixelCenterOffset - center.y;

                    if (offsetX * offsetX + offsetY * offsetY > squaredRadius)
                    {
                        continue;
                    }

                    var index = y * Width + x;

                    if (_pixels[index] == true)
                    {
                        continue;
                    }

                    _pixels[index] = true;
                    InkedPixelsCount++;
                }
            }
        }

        public void DrawLine(Vector2 from, Vector2 to, float radius)
        {
            var distance = Vector2.Distance(from, to);
            var step = Mathf.Max(radius * StepFactor, MinimumStep);
            var stepsCount = Mathf.CeilToInt(distance / step);

            DrawDot(from, radius);

            for (var index = 1; index <= stepsCount; index++)
            {
                var point = Vector2.Lerp(from, to, (float)index / stepsCount);

                DrawDot(point, radius);
            }
        }

        public void Clear()
        {
            System.Array.Clear(_pixels, 0, _pixels.Length);
            InkedPixelsCount = 0;
        }
    }
}

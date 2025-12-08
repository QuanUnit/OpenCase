using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedLayoutGroups.Implementation.Data;
using UnityEngine;

namespace AdvancedLayoutGroups.Tools.Layout
{
    public static class AdvancedLayoutTools
    {
        public static void CalculateLayoutElementsAlongAxisPositions(IEnumerable<RectTransform> elements, int elementsCount,
            OneAxisLayoutCalculationSettings calculationSettings, RectTransform container, RectTransform.Axis axis, Dictionary<RectTransform, Vector3> result)
        {
            result.Clear();

            Rect containerRect = ApplyPadding(container.rect, calculationSettings.Padding);
            Vector2 elementsBoundsSize = CalculateElementsBoundsSize(elements, elementsCount, calculationSettings.Spacing, axis);
            Rect elementsBounds = new Rect(Vector3.zero, elementsBoundsSize);
            elementsBounds = SnapRectToAnchor(elementsBounds, containerRect, calculationSettings.Anchor);
            
            CalculateLayoutElementAlongAxisData data = new CalculateLayoutElementAlongAxisData()
            {
                Anchor = calculationSettings.Anchor,
                ContainerRect = containerRect,
                ElementsBounds = elementsBounds,
                Space = calculationSettings.Spacing,
                Axis = axis,
            };

            CalculateLayoutElementsAlongAxisPositionsInternal(elements, data, result);
        }

        private static Rect ApplyPadding(Rect rect, Padding padding)
        {
            rect.min = rect.min + new Vector2(padding.Left, padding.Bottom);
            rect.max = rect.max - new Vector2(padding.Right, padding.Top);

            return rect;
        }
        
        private static void CalculateLayoutElementsAlongAxisPositionsInternal(IEnumerable<RectTransform> elements, 
            CalculateLayoutElementAlongAxisData data, Dictionary<RectTransform, Vector3> result)
        {
            int axisIndex = (int)data.Axis;
            int anotherAxisIndex = (axisIndex + 1) % 2;

            float currentAxisValue = data.ElementsBounds.min[axisIndex];
            
            foreach (var element in elements)
            {
                Vector2 size = GetElementSize(element);
                currentAxisValue += size[axisIndex] / 2;

                Vector2 position = Vector2.zero;
                
                position[axisIndex] = currentAxisValue;
                position[anotherAxisIndex] = GetSnappedAxisValueToRectBorder(size, data.ElementsBounds, data.Anchor, anotherAxisIndex);

                currentAxisValue += size[axisIndex] / 2;
                currentAxisValue += data.Space;
                
                result.Add(element, position);
            }

            float GetSnappedAxisValueToRectBorder(Vector2 elementSize, Rect containerRect, LayoutAnchor anchor, int axisIndex)
            {
                SeparateAnchorAxisValues(anchor, out int horizontalAnchorInt, out int verticalAnchorInt);

                bool isVerticalAxis = axisIndex == 1;
                bool isHorizontal = !isVerticalAxis;
                
                if (verticalAnchorInt == 0 && isVerticalAxis || horizontalAnchorInt == 2 && isHorizontal)
                {
                    return containerRect.max[axisIndex] - elementSize[axisIndex] / 2;
                }
                
                if (verticalAnchorInt == 2 && isVerticalAxis|| horizontalAnchorInt == 0 && isHorizontal)
                {
                    return containerRect.min[axisIndex] + elementSize[axisIndex] / 2;
                }

                return containerRect.center[axisIndex];
            }
        }

        private static Rect SnapRectToAnchor(Rect rect, Rect container, LayoutAnchor anchor)
        {
            float x;
            float y;

            SeparateAnchorAxisValues(anchor, out int horizontalAnchorInt, out int verticalAnchorInt);

            if (horizontalAnchorInt == 0) x = container.xMin + rect.width / 2;
            else if (horizontalAnchorInt == 1) x = container.center.x;
            else if (horizontalAnchorInt == 2) x = container.xMax - rect.width / 2;
            else throw new ArgumentOutOfRangeException($"Anchor value is invalid");
            
            if (verticalAnchorInt == 0) y = container.yMax - rect.height / 2;
            else if (verticalAnchorInt == 1) y = container.center.y;
            else if (verticalAnchorInt == 2) y = container.yMin + rect.height / 2;
            else throw new ArgumentOutOfRangeException($"Anchor value is invalid");

            rect.center = new Vector2(x, y);
            return rect;
        }

        public static Vector2 CalculateElementsBoundsSize(IEnumerable<RectTransform> elements, int elementsCount, float spacing, RectTransform.Axis axis)
        {
            if (elements.Any() == false)
            {
                return default;
            }
            
            Vector2 boundsSize = elements.First().rect.size;

            float fromSpacingLength = spacing * (elementsCount - 1);

            int axisAsInt = (int)axis;
            int anotherAxisAsInt = (axisAsInt + 1) % 2;
            
            foreach (var element in elements.Skip(1))
            {
                Vector2 elementSize = GetElementSize(element);
                
                if (elementSize[anotherAxisAsInt] > boundsSize[anotherAxisAsInt]) boundsSize[anotherAxisAsInt] = elementSize[anotherAxisAsInt];

                boundsSize[axisAsInt] += elementSize[axisAsInt];

            }

            boundsSize[axisAsInt] += fromSpacingLength;

            return boundsSize;
        }

        private static void SeparateAnchorAxisValues(LayoutAnchor anchor, out int horizontal, out int vertical)
        {
            vertical = (int)anchor / 3;
            horizontal = (int)anchor % 3;
        }

        public static Vector2 GetElementSize(RectTransform rectTransform)
        {
            return rectTransform.rect.size;
        }

        public static void PrepareRectTransformForCorrectLayout(RectTransform rectTransform)
        {
            Vector2 half = Vector2.one / 2;
            
            rectTransform.anchorMin = half;
            rectTransform.anchorMax = half;

            rectTransform.pivot = half;
        }
        
        private struct CalculateLayoutElementAlongAxisData
        {
            public int ElementsCount;
            public Rect ContainerRect;
            public Rect ElementsBounds;
            public LayoutAnchor Anchor;
            public float Space;
            public RectTransform.Axis Axis;
        }
    }
}
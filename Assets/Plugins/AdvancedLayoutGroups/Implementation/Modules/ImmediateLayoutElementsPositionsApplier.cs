using System;
using System.Collections.Generic;
using AdvancedLayoutGroups.Abstraction.Modules;
using AdvancedLayoutGroups.Tools.Layout;
using UnityEngine;

namespace AdvancedLayoutGroups.Implementation.Modules
{
    [Serializable]
    public class ImmediateLayoutElementsPositionsApplier : LayoutGroupModuleBase, ILayoutElementsPositionsApplier 
    {
        public void Apply(IReadOnlyDictionary<RectTransform, Vector3> targetPositions)
        {
            foreach (var pair in targetPositions)
            {
                RectTransform element = pair.Key;
                AdvancedLayoutTools.PrepareRectTransformForCorrectLayout(element);
                element.anchoredPosition = pair.Value;
            }
        }
    }
}
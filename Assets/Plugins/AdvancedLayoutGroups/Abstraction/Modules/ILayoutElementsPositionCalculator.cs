using System.Collections.Generic;
using UnityEngine;

namespace AdvancedLayoutGroups.Abstraction.Modules
{
    public interface ILayoutElementsPositionCalculator
    {
        public IReadOnlyDictionary<RectTransform, Vector3> Calculate(RectTransform container);
    }
}
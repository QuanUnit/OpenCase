using System.Collections.Generic;
using UnityEngine;

namespace AdvancedLayoutGroups.Abstraction.Modules
{
    public interface ILayoutElementsPositionsApplier
    {
        public void Apply(IReadOnlyDictionary<RectTransform, Vector3> targetPositions);
    }
}
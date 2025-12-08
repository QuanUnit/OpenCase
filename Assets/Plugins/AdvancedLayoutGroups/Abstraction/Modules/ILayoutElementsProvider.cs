using System.Collections.Generic;
using UnityEngine;

namespace AdvancedLayoutGroups.Abstraction.Modules
{
    public interface ILayoutElementsProvider
    {
        public IEnumerable<RectTransform> ProvideElements();
        public int ProvideElementsCount();
    }
}
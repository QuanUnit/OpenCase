using System.Collections.Generic;
using AdvancedLayoutGroups.Abstraction.Modules;
using UnityEngine;

namespace AdvancedLayoutGroups.Implementation.Modules
{
    public class ChildrenLayoutElementsProvider : ILayoutElementsProvider
    {
        private readonly RectTransform _parent;
        
        public ChildrenLayoutElementsProvider(RectTransform parent)
        {
            _parent = parent;
        }

        public IEnumerable<RectTransform> ProvideElements()
        {
            for (int i = 0; i < _parent.childCount; i++)
            {
                yield return (RectTransform)_parent.GetChild(i);
            }
        }

        public int ProvideElementsCount()
        {
            return _parent.childCount;
        }
    }
}
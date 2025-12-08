using System;
using System.Collections.Generic;
using AdvancedLayoutGroups.Abstraction.Modules;
using AdvancedLayoutGroups.Implementation.Data;
using AdvancedLayoutGroups.Tools.Layout;
using UnityEngine;

namespace AdvancedLayoutGroups.Implementation.Modules
{
    [Serializable]
    public class VerticalLayoutElementsPositionCalculator : LayoutGroupModuleBase, ILayoutElementsPositionCalculator
    {
        private OneAxisLayoutCalculationSettings Settings => _settingsProvider.Invoke();
        
        private readonly Func<OneAxisLayoutCalculationSettings> _settingsProvider;
        private readonly ILayoutElementsProvider _elementsProvider;
        private readonly Dictionary<RectTransform, Vector3> _positions = new Dictionary<RectTransform, Vector3>();

        public VerticalLayoutElementsPositionCalculator(ILayoutElementsProvider elementsProvider, Func<OneAxisLayoutCalculationSettings> settingsProvider)
        {
            _elementsProvider = elementsProvider;
            _settingsProvider = settingsProvider;
        }
        
        public IReadOnlyDictionary<RectTransform, Vector3> Calculate(RectTransform container)
        {   
            IEnumerable<RectTransform> elements = _elementsProvider.ProvideElements();
            int elementsCount = _elementsProvider.ProvideElementsCount();
            
            AdvancedLayoutTools.CalculateLayoutElementsAlongAxisPositions(elements, elementsCount, Settings, container, RectTransform.Axis.Vertical, _positions);
            return _positions;
        }
    }
}
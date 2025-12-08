using System;
using AdvancedLayoutGroups.Abstraction.Modules;
using UnityEngine;

namespace AdvancedLayoutGroups.Implementation.Modules
{
    public class DefaultLayoutElementsChangeNotifier : ILayoutElementsChangeNotifier
    {
        public event Action Changed;

        private readonly RectTransform _root;
        private readonly ILayoutElementsProvider _elementsProvider;

        private float _cachedLayoutStateSum;
        
        public DefaultLayoutElementsChangeNotifier(RectTransform root, ILayoutElementsProvider elementsProvider)
        {
            _root = root;
            _elementsProvider = elementsProvider;
        }
        
        public void Update(float deltaTime)
        {
            Rect rootRect = _root.rect;
            float layoutStateSum = rootRect.width + rootRect.height * 2;
            int elementIndex = 0;
            
            foreach (var element in _elementsProvider.ProvideElements())
            {
                Vector2 size = element.sizeDelta;

                float subSum = size.x + size.y * 2;
                subSum *= elementIndex + 1;

                layoutStateSum += subSum;
                elementIndex++;
            }

            if (_cachedLayoutStateSum != layoutStateSum)
            {
                _cachedLayoutStateSum = layoutStateSum;
                Changed?.Invoke();
            }
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shared.UI.Effects
{
    [Serializable]
    public class TabIndicatorItem
    {
        public RectTransform Rect => _rect;
        public Graphic[] Graphics => _graphics;

        [SerializeField] private RectTransform _rect;
        [SerializeField] private Graphic[] _graphics = Array.Empty<Graphic>();
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

namespace Shared.UI.Layout
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UniformScaleFitter : UIBehaviour
    {
        public RectTransform Target => _target;

        [SerializeField] private RectTransform _target;

        private float _designHeight;

        protected override void Awake()
        {
            base.Awake();

            _designHeight = _target.rect.height;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            Fit();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            Fit();
        }

        private void Fit()
        {
            var rect = ((RectTransform)transform).rect;

            if (rect.height <= 0f || _designHeight <= 0f)
            {
                return;
            }

            var scale = Mathf.Min(1f, rect.height / _designHeight);
            var center = new Vector2(0.5f, 0.5f);

            _target.anchorMin = center;
            _target.anchorMax = center;
            _target.pivot = center;
            _target.anchoredPosition = Vector2.zero;
            _target.sizeDelta = new Vector2(rect.width / scale, _designHeight);
            _target.localScale = new Vector3(scale, scale, 1f);
        }
    }
}

using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Shared.UI.Effects
{
    [DisallowMultipleComponent]
    public class StaggeredAppear : MonoBehaviour
    {
        [SerializeField] private float _step = 0.03f;
        [SerializeField] private int _maxStaggeredItems = 24;
        [SerializeField] private float _duration = 0.25f;
        [SerializeField] private Vector3 _restScale = Vector3.one;
        [SerializeField] private float _startScale = 0.92f;
        [SerializeField] private bool _fade = true;
        [SerializeField] private Ease _ease = Ease.OutCubic;
        [SerializeField] private bool _playOnEnable = true;
        [SerializeField] private bool _animateAddedChildren = true;

        private readonly Dictionary<Transform, Tween> _tweens = new Dictionary<Transform, Tween>();
        private readonly HashSet<Transform> _knownChildren = new HashSet<Transform>();
        private readonly List<Transform> _detachedChildren = new List<Transform>();
        private readonly List<Tween> _tweensToKill = new List<Tween>();
        private int _batchFrame = -1;
        private int _batchIndex;

        private void OnEnable()
        {
            if (_playOnEnable == true)
            {
                Play();
            }
            else
            {
                RememberChildren();
            }
        }

        private void OnDisable()
        {
            KillAll();
        }

        private void OnTransformChildrenChanged()
        {
            if (isActiveAndEnabled == false || _animateAddedChildren == false)
            {
                RememberChildren();
                return;
            }

            _knownChildren.RemoveWhere(child => child == null || child.parent != transform);
            RemoveDetachedTweens();

            foreach (Transform child in transform)
            {
                if (_knownChildren.Add(child) == true && child.gameObject.activeSelf == true)
                {
                    Animate(child, NextBatchIndex());
                }
            }
        }

        public void Play()
        {
            KillAll();
            _batchFrame = Time.frameCount;
            _batchIndex = 0;

            foreach (Transform child in transform)
            {
                _knownChildren.Add(child);

                if (child.gameObject.activeSelf == true)
                {
                    Animate(child, NextBatchIndex());
                }
            }
        }

        public void Complete()
        {
            KillAll();
        }

        private int NextBatchIndex()
        {
            if (_batchFrame != Time.frameCount)
            {
                _batchFrame = Time.frameCount;
                _batchIndex = 0;
            }

            var index = _batchIndex;
            _batchIndex++;

            return index;
        }

        private void Animate(Transform child, int index)
        {
            if (_tweens.TryGetValue(child, out var previous) == true)
            {
                previous.Kill();
            }

            var canvasGroup = GetCanvasGroup(child);
            var delay = Mathf.Min(index, _maxStaggeredItems) * _step;

            Apply(child, canvasGroup, 0f);

            Tween tween = null;
            tween = DOVirtual
                .Float(0f, 1f, _duration, progress => Apply(child, canvasGroup, progress))
                .SetDelay(delay)
                .SetEase(_ease)
                .SetUpdate(true)
                .SetLink(child.gameObject)
                .OnKill(() => FinishTween(child, canvasGroup, tween));

            _tweens[child] = tween;
        }

        private void FinishTween(Transform child, CanvasGroup canvasGroup, Tween tween)
        {
            Apply(child, canvasGroup, 1f);

            if (_tweens.TryGetValue(child, out var current) == true && current == tween)
            {
                _tweens.Remove(child);
            }
        }

        private CanvasGroup GetCanvasGroup(Transform child)
        {
            if (_fade == false)
            {
                return null;
            }

            var canvasGroup = child.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = child.gameObject.AddComponent<CanvasGroup>();
            }

            return canvasGroup;
        }

        private void Apply(Transform child, CanvasGroup canvasGroup, float progress)
        {
            if (child == null)
            {
                return;
            }

            child.localScale = _restScale * Mathf.LerpUnclamped(_startScale, 1f, progress);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = progress;
            }
        }

        private void RememberChildren()
        {
            _knownChildren.Clear();

            foreach (Transform child in transform)
            {
                _knownChildren.Add(child);
            }
        }

        private void RemoveDetachedTweens()
        {
            _detachedChildren.Clear();

            foreach (var child in _tweens.Keys)
            {
                if (child == null || child.parent != transform)
                {
                    _detachedChildren.Add(child);
                }
            }

            foreach (var child in _detachedChildren)
            {
                var tween = _tweens[child];
                _tweens.Remove(child);
                tween.Kill();
            }

            _detachedChildren.Clear();
        }

        private void KillAll()
        {
            _tweensToKill.Clear();
            _tweensToKill.AddRange(_tweens.Values);
            _tweens.Clear();

            foreach (var tween in _tweensToKill)
            {
                tween.Kill();
            }

            _tweensToKill.Clear();
        }
    }
}

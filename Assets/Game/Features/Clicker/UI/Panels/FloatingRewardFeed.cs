using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Features.Clicker.UI.Panels
{
    [DisallowMultipleComponent]
    public class FloatingRewardFeed : MonoBehaviour
    {
        [SerializeField] private TMP_Text _template;
        [SerializeField] private RectTransform _container;
        [SerializeField] private Vector2 _startPosition;
        [SerializeField] private float _horizontalSpread = 64f;
        [SerializeField] private float _riseDistance = 120f;
        [SerializeField] private float _duration = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _fadeDelayShare = 0.4f;
        [SerializeField] private Ease _riseEase = Ease.OutCubic;
        [SerializeField] private Ease _fadeEase = Ease.InQuad;

        private readonly List<TMP_Text> _freeItems = new List<TMP_Text>();
        private readonly Dictionary<TMP_Text, Tween> _activeItems = new Dictionary<TMP_Text, Tween>();

        private void OnDisable()
        {
            foreach (var pair in _activeItems)
            {
                pair.Value.Kill();
                pair.Key.gameObject.SetActive(false);
                _freeItems.Add(pair.Key);
            }

            _activeItems.Clear();
        }

        public void Spawn(string text)
        {
            var item = TakeItem();
            var rect = item.rectTransform;
            var offsetX = Random.Range(-_horizontalSpread, _horizontalSpread);
            var startPosition = _startPosition + new Vector2(offsetX, 0f);
            var fadeDelay = _duration * _fadeDelayShare;

            item.SetText(text);
            item.alpha = 1f;
            rect.anchoredPosition = startPosition;
            rect.SetAsLastSibling();
            item.gameObject.SetActive(true);

            var tween = DOTween.Sequence()
                .Append(DOVirtual.Float(0f, _riseDistance, _duration, offset => rect.anchoredPosition = startPosition + new Vector2(0f, offset)).SetEase(_riseEase))
                .Insert(fadeDelay, DOVirtual.Float(1f, 0f, _duration - fadeDelay, alpha => item.alpha = alpha).SetEase(_fadeEase))
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(() => ReturnItem(item));

            _activeItems.Add(item, tween);
        }

        private TMP_Text TakeItem()
        {
            if (_freeItems.Count == 0)
            {
                return Instantiate(_template, _container);
            }

            var lastIndex = _freeItems.Count - 1;
            var item = _freeItems[lastIndex];
            _freeItems.RemoveAt(lastIndex);

            return item;
        }

        private void ReturnItem(TMP_Text item)
        {
            _activeItems.Remove(item);
            item.gameObject.SetActive(false);
            _freeItems.Add(item);
        }
    }
}

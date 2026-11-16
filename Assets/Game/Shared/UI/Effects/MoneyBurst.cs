using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Shared.UI.Effects
{
    [DisallowMultipleComponent]
    public class MoneyBurst : MonoBehaviour
    {
        [SerializeField] private Image _template;
        [SerializeField] private RectTransform _container;
        [SerializeField] private MoneyBurstVariant[] _variants = Array.Empty<MoneyBurstVariant>();

        [Header("Emission")]
        [SerializeField] private Vector2 _origin;
        [SerializeField] private float _spawnRadius = 96f;
        [SerializeField] private float _directionAngle = 90f;
        [SerializeField, Range(0f, 180f)] private float _spreadAngle = 70f;
        [SerializeField] private Vector2 _speedRange = new Vector2(320f, 560f);
        [SerializeField] private float _gravity = -1100f;
        [SerializeField] private Vector2 _spinRange = new Vector2(-360f, 360f);

        [Header("Lifetime")]
        [SerializeField] private Vector2 _lifetimeRange = new Vector2(0.55f, 0.8f);
        [SerializeField] private Vector2 _startScaleRange = new Vector2(0.8f, 1.1f);
        [SerializeField] private float _endScale = 0.6f;
        [SerializeField, Range(0f, 1f)] private float _fadeStartShare = 0.55f;

        private readonly List<Image> _freeItems = new List<Image>();
        private readonly Dictionary<Image, Tween> _activeItems = new Dictionary<Image, Tween>();

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

        public void Play(int count)
        {
            for (var index = 0; index < count; index++)
            {
                Spawn();
            }
        }

        private void Spawn()
        {
            var variant = _variants[Random.Range(0, _variants.Length)];
            var angle = (_directionAngle + Random.Range(-_spreadAngle, _spreadAngle)) * Mathf.Deg2Rad;
            var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            var particle = new Particle()
            {
                Item = TakeItem(),
                Color = variant.Color,
                StartPosition = _origin + direction * _spawnRadius,
                Velocity = direction * Random.Range(_speedRange.x, _speedRange.y),
                StartRotation = Random.Range(0f, 360f),
                Spin = Random.Range(_spinRange.x, _spinRange.y),
                StartScale = Random.Range(_startScaleRange.x, _startScaleRange.y),
                Lifetime = Random.Range(_lifetimeRange.x, _lifetimeRange.y),
            };

            var rect = particle.Item.rectTransform;
            particle.Item.sprite = variant.Sprite;
            rect.sizeDelta = variant.Size;
            rect.SetAsLastSibling();

            ApplyState(particle, 0f);
            particle.Item.gameObject.SetActive(true);

            var tween = DOVirtual
                .Float(0f, particle.Lifetime, particle.Lifetime, time => ApplyState(particle, time))
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(() => ReturnItem(particle.Item));

            _activeItems.Add(particle.Item, tween);
        }

        private void ApplyState(Particle particle, float time)
        {
            var rect = particle.Item.rectTransform;
            var progress = time / particle.Lifetime;
            var fadeProgress = Mathf.InverseLerp(_fadeStartShare, 1f, progress);
            var fall = new Vector2(0f, 0.5f * _gravity * time * time);
            var scale = Mathf.Lerp(particle.StartScale, _endScale, progress);
            var color = particle.Color;

            rect.anchoredPosition = particle.StartPosition + particle.Velocity * time + fall;
            rect.localRotation = Quaternion.Euler(0f, 0f, particle.StartRotation + particle.Spin * time);
            rect.localScale = new Vector3(scale, scale, 1f);

            color.a *= 1f - fadeProgress;
            particle.Item.color = color;
        }

        private Image TakeItem()
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

        private void ReturnItem(Image item)
        {
            _activeItems.Remove(item);
            item.gameObject.SetActive(false);
            _freeItems.Add(item);
        }

        private struct Particle
        {
            public Image Item;
            public Color Color;
            public Vector2 StartPosition;
            public Vector2 Velocity;
            public float StartRotation;
            public float Spin;
            public float StartScale;
            public float Lifetime;
        }
    }
}

using System;
using Shared.Tools.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Features.Contracts.UI.Panels
{
    public class SignaturePad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RawImage _surface;
        [SerializeField] private int _textureWidth = 512;
        [SerializeField] private int _textureHeight = 192;
        [SerializeField] private float _brushRadius = 5f;
        [SerializeField] private Color _inkColor = Color.black;
        [SerializeField] private Color _paperColor = Color.clear;

        private InkCanvas _canvas;
        private Texture2D _texture;
        private Color32[] _buffer;
        private Color32 _inkColor32;
        private Color32 _paperColor32;
        private Vector2 _previousPoint;
        private bool _isDrawing;

        public event Action<float> CoverageChanged;

        public float Coverage
        {
            get
            {
                EnsureInitialized();

                return _canvas.Coverage;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnDisable()
        {
            _isDrawing = false;
        }

        private void EnsureInitialized()
        {
            if (_canvas != null)
            {
                return;
            }

            _inkColor32 = _inkColor;
            _paperColor32 = _paperColor;

            _canvas = new InkCanvas(_textureWidth, _textureHeight);
            _buffer = new Color32[_textureWidth * _textureHeight];
            _texture = new Texture2D(_textureWidth, _textureHeight, TextureFormat.RGBA32, false);
            _texture.filterMode = FilterMode.Bilinear;
            _texture.wrapMode = TextureWrapMode.Clamp;

            Array.Fill(_buffer, _paperColor32);
            _texture.SetPixels32(_buffer);
            _texture.Apply(false);

            _surface.texture = _texture;
        }

        private void OnDestroy()
        {
            if (_texture != null)
            {
                Destroy(_texture);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            EnsureInitialized();

            if (TryGetPoint(eventData, out var point) == false)
            {
                return;
            }

            _isDrawing = true;
            _previousPoint = point;

            ApplyStroke(point, point);
        }

        public void OnDrag(PointerEventData eventData)
        {
            EnsureInitialized();

            if (_isDrawing == false)
            {
                return;
            }

            if (TryGetPoint(eventData, out var point) == false)
            {
                return;
            }

            ApplyStroke(_previousPoint, point);

            _previousPoint = point;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDrawing = false;
        }

        public void Clear()
        {
            EnsureInitialized();

            var countBefore = _canvas.InkedPixelsCount;

            _canvas.Clear();

            Array.Fill(_buffer, _paperColor32);
            _texture.SetPixels32(_buffer);
            _texture.Apply(false);

            if (countBefore == 0)
            {
                return;
            }

            CoverageChanged?.Invoke(_canvas.Coverage);
        }

        private bool TryGetPoint(PointerEventData eventData, out Vector2 point)
        {
            var rectTransform = (RectTransform)_surface.transform;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint) == false)
            {
                point = default;

                return false;
            }

            var rect = rectTransform.rect;

            point = new Vector2(
                (localPoint.x - rect.xMin) / rect.width * _textureWidth,
                (localPoint.y - rect.yMin) / rect.height * _textureHeight);

            point.x = Mathf.Clamp(point.x, -_brushRadius, _textureWidth + _brushRadius);
            point.y = Mathf.Clamp(point.y, -_brushRadius, _textureHeight + _brushRadius);

            return true;
        }

        private void ApplyStroke(Vector2 from, Vector2 to)
        {
            var countBefore = _canvas.InkedPixelsCount;

            _canvas.DrawLine(from, to, _brushRadius);

            if (_canvas.InkedPixelsCount == countBefore)
            {
                return;
            }

            var minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(from.x, to.x) - _brushRadius));
            var maxX = Mathf.Min(_textureWidth - 1, Mathf.CeilToInt(Mathf.Max(from.x, to.x) + _brushRadius));
            var minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(from.y, to.y) - _brushRadius));
            var maxY = Mathf.Min(_textureHeight - 1, Mathf.CeilToInt(Mathf.Max(from.y, to.y) + _brushRadius));

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    if (_canvas.IsInked(x, y) == true)
                    {
                        _buffer[y * _textureWidth + x] = _inkColor32;
                    }
                }
            }

            _texture.SetPixels32(_buffer);
            _texture.Apply(false);

            CoverageChanged?.Invoke(_canvas.Coverage);
        }
    }
}

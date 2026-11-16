using System;
using UnityEngine;

namespace Shared.UI.Effects
{
    [Serializable]
    public class MoneyBurstVariant
    {
        public Sprite Sprite => _sprite;
        public Color Color => _color;
        public Vector2 Size => _size;

        [SerializeField] private Sprite _sprite;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private Vector2 _size = new Vector2(28f, 28f);
    }
}

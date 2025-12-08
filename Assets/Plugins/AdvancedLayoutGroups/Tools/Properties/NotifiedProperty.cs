using System;
using UnityEngine;

namespace AdvancedLayoutGroups.Tools.Properties
{
    [Serializable]
    [PropertyProvider(nameof(_value))]
    public class NotifiedProperty<T> : INotifiedProperty
    {
        public event Action Changed;

        public T Value
        {
            get { return _value; }
            set
            {
                T tmp = _value;

                _value = value;

                if (tmp.Equals(_value) == false)
                {
                    InvokePropertyChangeNotification();
                }
            }
        }

        [SerializeField] private T _value;

        public NotifiedProperty(T value)
        {
            _value = value;
        }

        public NotifiedProperty()
        {
            _value = default;
        }

        private void InvokePropertyChangeNotification()
        {
            Changed?.Invoke();
        }

        public static implicit operator T(NotifiedProperty<T> property)
        {
            if (property == null) return default;
            
            return property.Value;
        }
    }
}
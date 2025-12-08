using System;
using System.Collections.Generic;

namespace AdvancedLayoutGroups.Tools.Properties
{
    public class PropertiesCombinedNotifier : IDisposable
    {
        public event Action Changed; 
        
        private readonly IEnumerable<INotifiedProperty> _properties;
        private bool _disposed = false;
        
        public PropertiesCombinedNotifier(IEnumerable<INotifiedProperty> properties)
        {
            _properties = properties;

            Subscribe();
        }
        
        public PropertiesCombinedNotifier(params INotifiedProperty[] properties)
        {
            _properties = properties;
            Subscribe();
        }
        
        ~PropertiesCombinedNotifier()
        {
            Dispose(disposing: true);
        }

        private void Subscribe()
        {
            foreach (var property in _properties)
            {
                property.Changed += ChangeHandle;
            }
        }

        private void Unsubscribe()
        {
            foreach (var property in _properties)
            {
                property.Changed -= ChangeHandle;
            }
        }

        private void ChangeHandle()
        {
            Changed?.Invoke();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Unsubscribe();
            }

            _disposed = true;
        }
    }
}
using System;
using System.Collections.Generic;
using QCore.Tools.Identifiers;
using Services.Skins;

namespace Services.Inventory
{
    public class InventoryService : IInventoryService
    {
        public event Action<Skin> Added;
        public event Action<Skin> Removed;

        public IEnumerable<Skin> AllSkins => _skins.Values;

        private Dictionary<QGuid, Skin> _skins = new Dictionary<QGuid, Skin>();
        
        public void Add(Skin skin)
        {
            _skins.Add(skin.Id, skin);
            Added?.Invoke(skin);
        }

        public bool Remove(Skin skin)
        {
            return Remove(skin.Id);
        }

        public bool Remove(QGuid id)
        {
            if (_skins.Remove(id, out Skin skin))
            {
                Removed?.Invoke(skin);
                return true;
            }

            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using QCore.Tools.Identifiers;
using Features.Skins.Data;
using UnityEngine;

namespace Features.Inventory.Services
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
            if (_skins.Remove(id, out var skin))
            {
                Removed?.Invoke(skin);
                return true;
            }

            return false;
        }

        public Skin Get(QGuid id)
        {
            var has = TryGet(id, out var skin);

            if (has == false)
            {
                throw new MissingReferenceException($"Skin with id {id} is missing in inventory");
            }

            return skin;
        }

        public bool TryGet(QGuid id, out Skin skin)
        {
            return _skins.TryGetValue(id, out skin);
        }
    }
}
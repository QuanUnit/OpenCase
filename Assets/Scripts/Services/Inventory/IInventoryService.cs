using System;
using System.Collections.Generic;
using QCore.Tools.Identifiers;
using Services.Skins;

namespace Services.Inventory
{
    public interface IInventoryService
    {
        public event Action<Skin> Added; 
        public event Action<Skin> Removed; 
        
        public IEnumerable<Skin> AllSkins { get; }
        
        public void Add(Skin skin);
        public bool Remove(Skin skin);
        public bool Remove(QGuid id);
        public Skin Get(QGuid id);
        public bool TryGet(QGuid id, out Skin skin);
    }
}
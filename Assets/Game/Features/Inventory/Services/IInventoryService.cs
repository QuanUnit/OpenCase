using System;
using System.Collections.Generic;
using Features.Skins.Data;

namespace Features.Inventory.Services
{
    public interface IInventoryService
    {
        public event Action<Skin> Added; 
        public event Action<Skin> Removed; 
        
        public IEnumerable<Skin> AllSkins { get; }
        
        public void Add(Skin skin);
        public bool Remove(Skin skin);
        public bool Remove(SkinInstanceId id);
        public Skin Get(SkinInstanceId id);
        public bool TryGet(SkinInstanceId id, out Skin skin);
    }
}
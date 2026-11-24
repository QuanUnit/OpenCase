using System.Collections.Generic;
using Features.Skins.Data;

namespace Features.Inventory.Data
{
    public class SelectedSkinsBuffer
    {
        public IEnumerable<Skin> Skins => _skins;

        private HashSet<Skin> _skins = new HashSet<Skin>();
        
        public void AddSelected(Skin skin)
        {
            _skins.Add(skin);
        }

        public void RemoveSelected(Skin skin)
        {
            _skins.Remove(skin);
        }

        public void Clear()
        {
            _skins.Clear();
        }
    }
}

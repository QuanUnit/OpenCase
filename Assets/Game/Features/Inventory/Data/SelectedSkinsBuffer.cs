using System.Collections.Generic;
using Features.Skins.Data;

namespace Features.Inventory.Data
{
    public class SelectedSkinsBuffer
    {
        public float TotalPrice { get; private set; }
        public IEnumerable<Skin> Skins => _skins;

        private HashSet<Skin> _skins = new HashSet<Skin>();
        
        public void AddSelected(Skin skin)
        {
            var added = _skins.Add(skin);

            if (added == false) return;

            TotalPrice += skin.Price;
        }

        public void RemoveSelected(Skin skin)
        {
            var removed = _skins.Remove(skin);

            if (removed == false) return;

            TotalPrice -= skin.Price;
        }

        public void Clear()
        {
            _skins.Clear();
            TotalPrice = 0f;
        }
    }
}
using System.Collections.Generic;
using Services.Skins;

namespace GameHub.Inventory.Presenters
{
    public class SelectedSkinsBuffer
    {
        public float TotalPrice { get; private set; }
        public IEnumerable<Skin> Skins => _skins;

        private HashSet<Skin> _skins = new HashSet<Skin>();
        
        public void AddSelected(Skin skin)
        {
            _skins.Add(skin);
            TotalPrice += skin.Price;
        }

        public void RemoveSelected(Skin skin)
        {
            bool removed = _skins.Remove(skin);

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
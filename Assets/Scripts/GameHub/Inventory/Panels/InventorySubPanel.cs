using System.Collections.Generic;
using System.Linq;
using QCore.Tools.Identifiers;
using QCore.UISystem.Panels;
using Services.Skins;
using UnityEngine;

namespace GameHub.Inventory.Panels
{
    public class InventorySubPanel : Panel
    {
        [SerializeField] private Transform _skinsViewport;
        [SerializeField] private SkinView _skinViewPrefab;

        private readonly Dictionary<QGuid, SkinView> _activeViews = new Dictionary<QGuid, SkinView>();
        
        public void AddSkinViews(IEnumerable<Skin> skins, CreateSkinViewAction createAction)
        {
            foreach (var skin in skins)
            {
                AddSkinView(skin, createAction);
            }
        }
        
        public void AddSkinView(Skin skin, CreateSkinViewAction createAction)
        {
            SkinView created = createAction.Invoke(skin, _skinViewPrefab, _skinsViewport);
            _activeViews.Add(skin.Id, created);
        }
        
        public void RemoveSkinView(QGuid id)
        {
            bool removed = _activeViews.Remove(id, out SkinView view);

            if(removed == false) return;
            Destroy(view.gameObject);
        }

        public void RemoveAllSkinViews()
        {
            while (_activeViews.Count > 0)
            {
                RemoveSkinView(_activeViews.First().Key);
            }
        }

        public delegate SkinView CreateSkinViewAction(Skin skin, SkinView prefab, Transform parent);
    }
}
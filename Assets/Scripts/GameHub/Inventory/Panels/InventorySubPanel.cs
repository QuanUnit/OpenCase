using System;
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
        public SkinDetailsPopup SkinDetailsPopup => _skinDetailsPopup;
        public event Action<QGuid> SkinClicked; 
        
        [SerializeField] private Transform _skinsViewport;
        [SerializeField] private SkinView _skinViewPrefab;
        [SerializeField] private SkinDetailsPopup _skinDetailsPopup;

        private readonly Dictionary<QGuid, SkinView> _activeViews = new Dictionary<QGuid, SkinView>();
        private readonly Dictionary<SkinView, QGuid> _viewToSkinIdMap = new Dictionary<SkinView, QGuid>();
        
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

            created.Clicked += SkinClickHandle;
            _activeViews.Add(skin.Id, created);
            _viewToSkinIdMap.Add(created, skin.Id);
        }

        private void SkinClickHandle(SkinView sender)
        {
            QGuid skinId = GetSkinId(sender);
            SkinClicked?.Invoke(skinId);
        }

        public void RemoveSkinView(QGuid id)
        {
            bool removed = _activeViews.Remove(id, out SkinView view);

            if(removed == false) return;
            
            _viewToSkinIdMap.Remove(view);

            view.Clicked -= SkinClickHandle;
            Destroy(view.gameObject);
        }

        public void RemoveAllSkinViews()
        {
            while (_activeViews.Count > 0)
            {
                RemoveSkinView(_activeViews.First().Key);
            }
        }

        public QGuid GetSkinId(SkinView view)
        {
            return _viewToSkinIdMap[view];
        }

        public delegate SkinView CreateSkinViewAction(Skin skin, SkinView prefab, Transform parent);
    }
}
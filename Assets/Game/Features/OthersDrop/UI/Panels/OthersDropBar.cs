using System.Collections.Generic;
using QCore.UISystem.Panels;
using Features.Skins.Data;
using UnityEngine;

namespace Features.OthersDrop.UI.Panels
{
    public class OthersDropBar : Panel
    {
        [SerializeField] private SkinView _skinViewPrefab;
        [SerializeField] private Transform _viewport;
        [SerializeField] private Transform _startPosition;
        
        [SerializeField] private int _skinsHistorySize = 10;

        private Queue<SkinView> _skinsHistory = new Queue<SkinView>();
        
        public void AddSkinView(Skin skin, CreateSkinViewAction createAction)
        {
            ClearHistoryIfOutOfSize();
            var skinView = createAction.Invoke(skin, _skinViewPrefab, _viewport, _startPosition);
            _skinsHistory.Enqueue(skinView);
        }

        private void ClearHistoryIfOutOfSize()
        {
            if (_skinsHistory.Count <= _skinsHistorySize) return;

            var forRemoveCount = _skinsHistory.Count - _skinsHistorySize;

            for (var i = 0; i < forRemoveCount; i++)
            {
                var view = _skinsHistory.Dequeue();
                Destroy(view.gameObject);
            }
        }

        public delegate SkinView CreateSkinViewAction(Skin skin, SkinView prefab, Transform parent, Transform startPosition);
    }
}
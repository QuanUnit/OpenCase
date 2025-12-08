using System.Collections.Generic;
using QCore.UISystem.Panels;
using Services.Skins;
using UnityEngine;

namespace GameHub.OthersDrop.Panels
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
            SkinView skinView = createAction.Invoke(skin, _skinViewPrefab, _viewport, _startPosition);
            _skinsHistory.Enqueue(skinView);
        }

        private void ClearHistoryIfOutOfSize()
        {
            if (_skinsHistory.Count <= _skinsHistorySize) return;

            int forRemoveCount = _skinsHistory.Count - _skinsHistorySize;

            for (int i = 0; i < forRemoveCount; i++)
            {
                SkinView view = _skinsHistory.Dequeue();
                Destroy(view.gameObject);
            }
        }

        public delegate SkinView CreateSkinViewAction(Skin skin, SkinView prefab, Transform parent, Transform startPosition);
    }
}
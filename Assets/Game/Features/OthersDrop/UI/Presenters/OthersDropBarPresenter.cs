using Features.OthersDrop.UI.Panels;
using QCore.Tools.Timers;
using QCore.UISystem.Presenters;
using Features.Skins.Data;
using Features.Skins.Services;
using Features.Skins.Configs;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Features.OthersDrop.UI.Presenters
{
    public class OthersDropBarPresenter : PanelPresenter<OthersDropBar>
    {
        private readonly ISkinsDatabaseService _skinsDatabaseService;
        private readonly ISkinsUIContextDataService _skinsUIContextDataService;
        private readonly ISkinFactory _skinFactory;
        
        private const float MinCooldown = 0.5f;
        private const float MaxCooldown = 2f;

        private AsyncTimer _asyncTimer;

        public OthersDropBarPresenter(ISkinsDatabaseService skinsDatabaseService, ISkinsUIContextDataService skinsUIContextDataService, ISkinFactory skinFactory)
        {
            _skinsDatabaseService = skinsDatabaseService;
            _skinsUIContextDataService = skinsUIContextDataService;
            _skinFactory = skinFactory;
        }
        
        protected override void StartPresentingInternal()
        {
            _asyncTimer = new AsyncTimer(1f, AsyncTimer.TypeTimer.Single);

            _asyncTimer.ChangeDuration(GetRandomCooldown());
            _asyncTimer.Launch();
            _asyncTimer.OnComplete += TimerCompleteHandle;
        }

        private void TimerCompleteHandle(AsyncTimer sender)
        {
            _asyncTimer.ChangeDuration(GetRandomCooldown());
            _asyncTimer.Launch();
            AddSkinToPanel();
        }

        private void AddSkinToPanel()
        {
            var skinConfig = _skinsDatabaseService.GetRandomSkinConfig();
            var skin = _skinFactory.CreateRandom(skinConfig);
            
            Panel.AddSkinView(skin, CreateSkinView);
        }

        private SkinView CreateSkinView(Skin skin, SkinView prefab, Transform parent, Transform startPosition)
        {
            var view = Object.Instantiate(prefab, startPosition.position, startPosition.rotation, parent);
            var config = _skinsDatabaseService.GetSkinConfigById(skin.SkinSourceId);
            var rarityVisual = _skinsUIContextDataService.GetRarityVisual(config.Rarity);
            
            view.SetView(new SkinView.Data()
            {
                Name = config.Name,
                RarityBar = rarityVisual.RarityMainColor,
                SkinIcon = config.ContextData.SpriteSource.GetAsset(),
            });

            return view;
        }

        protected override void StopPresentingInternal()
        {
            _asyncTimer.Stop();
            _asyncTimer.OnComplete -= TimerCompleteHandle;
        }

        private float GetRandomCooldown()
        {
            return Random.Range(MinCooldown, MaxCooldown);
        }
    }
}
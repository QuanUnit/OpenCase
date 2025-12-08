using GameHub.OthersDrop.Panels;
using QCore.Tools.Timers;
using QCore.UISystem.Presenters;
using Services.Skins;
using Services.Skins.Configs;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GameHub.OthersDrop.Presenters
{
    public class OthersDropBarPresenter : PanelPresenter<OthersDropBar>
    {
        [Inject] private ISkinsDatabaseService _skinsDatabaseService;
        [Inject] private ISkinsUIContextDataService _skinsUIContextDataService;
        [Inject] private ISkinFactory _skinFactory;
        
        private const float MinCooldown = 0.5f;
        private const float MaxCooldown = 2f;

        private AsyncTimer _asyncTimer;
        
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
            SkinConfig skinConfig = _skinsDatabaseService.GetRandomSkinConfig();
            Skin skin = _skinFactory.CreateRandom(skinConfig);
            
            Panel.AddSkinView(skin, CreateSkinView);
        }

        private SkinView CreateSkinView(Skin skin, SkinView prefab, Transform parent, Transform startPosition)
        {
            SkinView view = Object.Instantiate(prefab, startPosition.position, startPosition.rotation, parent);
            SkinConfig config = _skinsDatabaseService.GetSkinConfigById(skin.SkinSourceId);
            RarityVisual rarityVisual = _skinsUIContextDataService.GetRarityVisual(config.Rarity);
            
            view.SetView(new SkinView.Data()
            {
                Name = config.Name,
                RarityBackground = rarityVisual.RarityBackgroundColor,
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
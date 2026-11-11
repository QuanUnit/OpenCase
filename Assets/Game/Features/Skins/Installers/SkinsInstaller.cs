using Features.Skins.Services;
using Features.Skins.Configs;
using Shared.Lifecycle;
using UnityEngine;
using Zenject;

namespace Features.Skins.Installers
{
    public class SkinsInstaller : MonoInstaller
    {
        [SerializeField] private AllSkinsConfig _allSkinsConfig;
        [SerializeField] private SkinsGlobalSettingsConfig _skinsGlobalSettingsConfig;
        [SerializeField] private SkinsUIContextDataConfig _skinsUIContextDataConfig;
        
        public override void InstallBindings()
        {
            Container.BindInstance(_allSkinsConfig).AsSingle();
            Container.BindInstance(_skinsGlobalSettingsConfig).AsSingle();
            Container.BindInstance(_skinsUIContextDataConfig).AsSingle();
            
            Container.Bind(typeof(ISkinsDatabaseService), typeof(IInitializableService), typeof(IAssetsLoader)).To<SkinsDatabaseService>().AsSingle();
            Container.Bind(typeof(ISkinsUIContextDataService), typeof(IInitializableService)).To<SkinsUIContextDataService>().AsSingle();
            Container.Bind<ISkinFactory>().To<SkinFactory>().AsSingle();
        }
    }
}
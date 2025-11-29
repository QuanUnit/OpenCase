using Services.Skins;
using Services.Skins.Configs;
using UnityEngine;
using Zenject;

namespace Kernel.Installers
{
    public class SkinsEnvironmentInstaller : MonoInstaller
    {
        [SerializeField] private AllSkinsConfig _allSkinsConfig;
        [SerializeField] private SkinsGlobalSettingsConfig _skinsGlobalSettingsConfig;
        [SerializeField] private SkinsUIContextDataConfig _skinsUIContextDataConfig;
        
        public override void InstallBindings()
        {
            Container.BindInstance(_allSkinsConfig).AsSingle();
            Container.BindInstance(_skinsGlobalSettingsConfig).AsSingle();
            Container.BindInstance(_skinsUIContextDataConfig).AsSingle();
            
            Container.Bind<ISkinsDatabaseService>().To<SkinsDatabaseService>().AsSingle();
            Container.Bind<ISkinsUIContextDataService>().To<SkinsUIContextDataService>().AsSingle();
            Container.Bind<ISkinFactory>().To<SkinFactory>().AsSingle();
        }
    }
}
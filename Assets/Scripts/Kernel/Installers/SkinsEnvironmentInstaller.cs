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
        
        public override void InstallBindings()
        {
            Container.BindInstance(_allSkinsConfig).AsSingle();
            Container.BindInstance(_skinsGlobalSettingsConfig).AsSingle();
            
            Container.Bind<ISkinsDatabaseService>().To<SkinsDatabaseService>().AsSingle();
            Container.Bind<ISkinFactory>().To<SkinFactory>().AsSingle();
        }
    }
}
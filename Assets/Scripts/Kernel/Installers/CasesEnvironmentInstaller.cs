using Services.Cases;
using Services.Cases.Configs;
using UnityEngine;
using Zenject;

namespace Kernel.Installers
{
    public class CasesEnvironmentInstaller : MonoInstaller
    {
        [SerializeField] private AllCasesConfig _allCasesConfig;
        
        public override void InstallBindings()
        {
            Container.BindInstance(_allCasesConfig).AsSingle();

            Container.Bind<ICasesDatabaseService>().To<CasesDatabaseService>().AsSingle();
        }
    }
}
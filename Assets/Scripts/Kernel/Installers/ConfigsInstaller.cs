using Kernel.Configs;
using Services.Skins.Configs;
using UnityEngine;
using Zenject;

namespace Kernel.Installers
{
    public class ConfigsInstaller : MonoInstaller
    {
        [SerializeField] private ProjectKernelConfig _projectKernelConfig; 
        [SerializeField] private AllCasesConfig _allCasesConfig; 
        
        public override void InstallBindings()
        {
            BindKernelConfigs();
            BindCasesConfigs();
        }

        private void BindCasesConfigs()
        {
            Container.Bind<AllCasesConfig>().FromInstance(_allCasesConfig).AsSingle();
        }

        private void BindKernelConfigs()
        {
            Container.Bind<ProjectKernelConfig>().FromInstance(_projectKernelConfig).AsSingle();
        }
    }
}
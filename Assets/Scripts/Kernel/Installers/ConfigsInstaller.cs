using Kernel.Configs;
using UnityEngine;
using Zenject;

namespace Kernel.Installers
{
    public class ConfigsInstaller : MonoInstaller
    {
        [SerializeField] private ProjectKernelConfig _projectKernelConfig;

        public override void InstallBindings()
        {
            BindKernelConfigs();
        }

        private void BindKernelConfigs()
        {
            Container.BindInstance(_projectKernelConfig).AsSingle();
        }
    }
}
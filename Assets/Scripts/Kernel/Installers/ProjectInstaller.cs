using Kernel.Flow;
using Zenject;

namespace Kernel.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<GlobalStateMachineFactory>().AsSingle();
        }
    }
}

using Features.Contracts.Configs;
using Features.Contracts.Services;
using Features.Contracts.UI.Panels;
using Features.Contracts.UI.Presenters;
using Shared.UI.PanelPresenters;
using UnityEngine;
using Zenject;

namespace Features.Contracts.Installers
{
    public class ContractsInstaller : MonoInstaller
    {
        [SerializeField] private ContractConfig _contractConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(_contractConfig).AsSingle();

            Container.Bind<IContractRandom>().To<ContractRandom>().AsSingle();
            Container.Bind<IContractService>().To<ContractService>().AsSingle();

            Container.BindPanelPresenter<ContractSubPanel, ContractSubPanelPresenter>();
        }
    }
}

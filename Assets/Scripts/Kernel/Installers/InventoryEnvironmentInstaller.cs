using Services.Inventory;
using Services.Wallet;
using Services.Wallet.Configs;
using UnityEngine;
using Zenject;

namespace Kernel.Installers
{
    public class InventoryEnvironmentInstaller : MonoInstaller
    {
        [SerializeField] private SoftCurrencyWalletConfig _softCurrencyWalletConfig;
        
        public override void InstallBindings()
        {
            Container.Bind<IInventoryService>().To<InventoryService>().AsSingle();
            Container.BindInterfacesAndSelfTo<SoftCurrencyWallet>().AsSingle();

            Container.BindInstance(_softCurrencyWalletConfig).AsSingle();
        }
    }
}
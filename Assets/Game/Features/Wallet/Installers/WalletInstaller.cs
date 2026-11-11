using Features.Wallet.Configs;
using Features.Wallet.Services;
using UnityEngine;
using Zenject;

namespace Features.Wallet.Installers
{
    public class WalletInstaller : MonoInstaller
    {
        [SerializeField] private SoftCurrencyWalletConfig _softCurrencyWalletConfig;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SoftCurrencyWallet>().AsSingle();

            Container.BindInstance(_softCurrencyWalletConfig).AsSingle();
        }
    }
}

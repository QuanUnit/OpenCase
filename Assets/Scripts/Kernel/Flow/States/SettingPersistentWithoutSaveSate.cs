using QCore.StateMachine.States;
using Services.Wallet;
using Services.Wallet.Configs;
using Zenject;

namespace Kernel.Flow.States
{
    public class SettingPersistentWithoutSaveSate : State
    {
        [Inject] private SoftCurrencyWalletConfig _softCurrencyWalletConfig;
        
        [Inject] private SoftCurrencyWallet _softCurrencyWallet;
        
        public override void Enter()
        {
            Init();
            StateMachine.EnterState<LoadingAssetsState>();
        }

        private void Init()
        {
            _softCurrencyWallet.Add(_softCurrencyWalletConfig.DefaultValue);
        }
    }
}
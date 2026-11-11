using QCore.StateMachine.States;
using Features.Wallet.Services;
using Features.Wallet.Configs;

namespace Kernel.Flow.States
{
    public class SettingPersistentWithoutSaveState : State
    {
        private readonly SoftCurrencyWalletConfig _softCurrencyWalletConfig;
        
        private readonly SoftCurrencyWallet _softCurrencyWallet;

        public SettingPersistentWithoutSaveState(SoftCurrencyWalletConfig softCurrencyWalletConfig, SoftCurrencyWallet softCurrencyWallet)
        {
            _softCurrencyWalletConfig = softCurrencyWalletConfig;
            _softCurrencyWallet = softCurrencyWallet;
        }
        
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
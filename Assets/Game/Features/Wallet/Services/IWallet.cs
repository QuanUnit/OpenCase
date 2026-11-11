namespace Features.Wallet.Services
{
    public interface IWallet
    {
        public event WalletTransactionAction Changed;
        
        public float Currency { get; }

        public void Add(float value);
        public void Spend(float value);
        public bool TrySpend(float value);
        public bool IsEnoughForSpend(float value);
    }

    public delegate void WalletTransactionAction(IWallet sender, float delta, float current);
}
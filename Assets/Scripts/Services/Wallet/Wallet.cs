using System;

namespace Services.Wallet
{
    public abstract class Wallet : IWallet
    {
        public event WalletTransactionAction Changed;
        
        public float Currency { get; private set; }
        
        public void Add(float value)
        {
            ValidateAndThrowIfNegative(value);

            Currency += value;
            Changed?.Invoke(this, value, Currency);
        }

        public void Spend(float value)
        {
            ValidateAndThrowIfNegative(value);

            if (IsEnoughForSpend(value) == false)
            {
                throw new InvalidOperationException("Not enough money");
            }
            
            Currency -= value;
            Changed?.Invoke(this, -value, Currency);
        }

        public bool TrySpend(float value)
        {
            ValidateAndThrowIfNegative(value);

            if (IsEnoughForSpend(value) == false)
            {
                return false;
            }
            
            Currency -= value;
            Changed?.Invoke(this, -value, Currency);
            return true;
        }

        public bool IsEnoughForSpend(float value)
        {
            return Currency >= value;
        }
        
        private void ValidateAndThrowIfNegative(float value)
        {
            if (value <= 0)
            {
                throw new InvalidOperationException("Value is invalid");
            }
        }
    }
}
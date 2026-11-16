using Features.Clicker.Configs;
using Features.Wallet.Services;
using Shared.Tools.Mathematics;
using UnityEngine;

namespace Features.Clicker.Services
{
    public class ClickerService : IClickerService
    {
        private readonly SoftCurrencyWallet _softCurrencyWallet;
        private readonly ClickerConfig _clickerConfig;

        public float HoldInterval => _clickerConfig.HoldInterval;

        public ClickerService(SoftCurrencyWallet softCurrencyWallet, ClickerConfig clickerConfig)
        {
            _softCurrencyWallet = softCurrencyWallet;
            _clickerConfig = clickerConfig;
        }

        public float Earn()
        {
            var balance = _softCurrencyWallet.Currency;
            var target = MoneyMath.RoundToMoney(balance + _clickerConfig.RewardPerClick);
            var reward = target - balance;

            if (reward <= 0f)
            {
                return 0f;
            }

            _softCurrencyWallet.Add(reward);

            return reward;
        }

        public int GetParticlesCount(float amount)
        {
            var cents = MoneyMath.ToMinorUnits(amount);
            var count = Mathf.RoundToInt(cents * _clickerConfig.ParticlesPerCent);

            return Mathf.Clamp(count, _clickerConfig.MinParticles, _clickerConfig.MaxParticles);
        }
    }
}

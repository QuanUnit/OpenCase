using System;
using QCore.UISystem.Panels;
using Shared.UI.Effects;
using UnityEngine;

namespace Features.Clicker.UI.Panels
{
    public class ClickerSubPanel : Panel
    {
        public event Action TargetPressed;
        public event Action TargetReleased;

        [SerializeField] private ClickerTarget _target;
        [SerializeField] private ScalePunch _targetPunch;
        [SerializeField] private FloatingRewardFeed _rewardFeed;
        [SerializeField] private MoneyBurst _moneyBurst;
        [SerializeField] private string _rewardPrefix = "+";

        private void OnEnable()
        {
            _target.Pressed += TargetPressHandle;
            _target.Released += TargetReleaseHandle;
        }

        public void PlayReward(float amount, int particlesCount)
        {
            _targetPunch.Play();
            _moneyBurst.Play(particlesCount);
            _rewardFeed.Spawn(_rewardPrefix + NumberCounterFormatter.FormatMoney(amount));
        }

        private void TargetPressHandle()
        {
            TargetPressed?.Invoke();
        }

        private void TargetReleaseHandle()
        {
            TargetReleased?.Invoke();
        }

        private void OnDisable()
        {
            _target.Pressed -= TargetPressHandle;
            _target.Released -= TargetReleaseHandle;
        }
    }
}

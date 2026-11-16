using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Clicker.Services;
using Features.Clicker.UI.Panels;
using QCore.UISystem.Presenters;
using UnityEngine;

namespace Features.Clicker.UI.Presenters
{
    public class ClickerSubPanelPresenter : PanelPresenter<ClickerSubPanel>
    {
        private readonly IClickerService _clickerService;

        private CancellationTokenSource _holdCancellation;

        public ClickerSubPanelPresenter(IClickerService clickerService)
        {
            _clickerService = clickerService;
        }

        protected override void StartPresentingInternal()
        {
            Panel.TargetPressed += TargetPressHandle;
            Panel.TargetReleased += TargetReleaseHandle;
        }

        private void TargetPressHandle()
        {
            StopHold();

            _holdCancellation = new CancellationTokenSource();
            Hold(_holdCancellation.Token).Forget();
        }

        private void TargetReleaseHandle()
        {
            StopHold();
        }

        private async UniTaskVoid Hold(CancellationToken cancellationToken)
        {
            var interval = _clickerService.HoldInterval;
            var nextRewardTime = Time.unscaledTime;

            while (true)
            {
                Reward();

                if (cancellationToken.IsCancellationRequested == true)
                {
                    return;
                }

                nextRewardTime = Mathf.Max(nextRewardTime + interval, Time.unscaledTime);

                var delay = TimeSpan.FromSeconds(nextRewardTime - Time.unscaledTime);
                var isCanceled = await UniTask
                    .Delay(delay, DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, cancellationToken)
                    .SuppressCancellationThrow();

                if (isCanceled == true)
                {
                    return;
                }
            }
        }

        private void Reward()
        {
            var amount = _clickerService.Earn();

            if (amount > 0f)
            {
                Panel.PlayReward(amount, _clickerService.GetParticlesCount(amount));
            }
        }

        private void StopHold()
        {
            if (_holdCancellation == null)
            {
                return;
            }

            _holdCancellation.Cancel();
            _holdCancellation.Dispose();
            _holdCancellation = null;
        }

        protected override void StopPresentingInternal()
        {
            Panel.TargetPressed -= TargetPressHandle;
            Panel.TargetReleased -= TargetReleaseHandle;

            StopHold();
        }
    }
}

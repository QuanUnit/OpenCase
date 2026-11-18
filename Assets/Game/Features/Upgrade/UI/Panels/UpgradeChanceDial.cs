using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Upgrade.UI.Panels
{
    public class UpgradeChanceDial : MonoBehaviour
    {
        private const float FullTurnDegrees = 360f;

        [SerializeField] private Image _successZone;
        [SerializeField] private RectTransform _pointer;
        [SerializeField] private TMP_Text _chanceText;
        [SerializeField] private string _chanceFormat = "{0:0.0}%";
        [SerializeField] private string _emptyChanceText = "-";
        [SerializeField, Range(0f, 0.5f)] private float _landingEdgeMargin = 0.15f;
        [SerializeField] private Ease _rollEase = Ease.OutQuart;

        private Tween _rollTween;

        public void SetChance(float chance)
        {
            _successZone.fillAmount = chance;
            _chanceText.text = string.Format(CultureInfo.InvariantCulture, _chanceFormat, chance * 100f);
        }

        public void SetEmpty()
        {
            _successZone.fillAmount = 0f;
            _chanceText.text = _emptyChanceText;
        }

        public void ResetPointer()
        {
            _rollTween?.Kill();
            _pointer.localEulerAngles = Vector3.zero;
        }

        public async UniTask PlayRoll(float chance, bool isSuccess, int turns, float duration, CancellationToken cancellationToken)
        {
            ResetPointer();

            var landing = Random.Range(_landingEdgeMargin, 1f - _landingEdgeMargin);
            var fraction = isSuccess == true
                ? Mathf.Lerp(0f, chance, landing)
                : Mathf.Lerp(chance, 1f, landing);

            var angle = -(turns + fraction) * FullTurnDegrees;

            _rollTween = _pointer
                .DOLocalRotate(new Vector3(0f, 0f, angle), duration, RotateMode.FastBeyond360)
                .SetEase(_rollEase)
                .SetUpdate(true)
                .SetLink(gameObject);

            await _rollTween.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
        }

        private void OnDisable()
        {
            ResetPointer();
        }
    }
}

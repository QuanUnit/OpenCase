using UnityEngine;

namespace Features.Cases.Configs
{
    [CreateAssetMenu(menuName = "Configs/Cases/" + nameof(CaseSpinConfig), fileName = nameof(CaseSpinConfig))]
    public class CaseSpinConfig : ScriptableObject
    {
        public int MinCount => _minCount;
        public int MaxCount => _maxCount;
        public float SpinStagger => _spinStagger;

        [SerializeField, Min(1)] private int _minCount = 1;
        [SerializeField, Min(1)] private int _maxCount = 10;
        [SerializeField, Min(0f)] private float _spinStagger = 0.12f;

        private void OnValidate()
        {
            _minCount = Mathf.Max(1, _minCount);
            _maxCount = Mathf.Max(_minCount, _maxCount);
        }
    }
}

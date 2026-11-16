using UnityEngine;

namespace Features.Clicker.Configs
{
    [CreateAssetMenu(menuName = "Configs/Clicker/" + nameof(ClickerConfig), fileName = nameof(ClickerConfig))]
    public class ClickerConfig : ScriptableObject
    {
        public float RewardPerClick => _rewardPerClick;
        public float HoldInterval => _holdInterval;
        public float ParticlesPerCent => _particlesPerCent;
        public int MinParticles => _minParticles;
        public int MaxParticles => _maxParticles;

        [SerializeField, Min(0.01f)] private float _rewardPerClick = 0.01f;
        [SerializeField, Min(0.05f)] private float _holdInterval = 0.15f;

        [Header("Money burst")]
        [SerializeField, Min(0f)] private float _particlesPerCent = 4f;
        [SerializeField, Min(0)] private int _minParticles = 3;
        [SerializeField, Min(0)] private int _maxParticles = 12;

        private void OnValidate()
        {
            _maxParticles = Mathf.Max(_minParticles, _maxParticles);
        }
    }
}

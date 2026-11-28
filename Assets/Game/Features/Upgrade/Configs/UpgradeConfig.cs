using System.Collections.Generic;
using UnityEngine;

namespace Features.Upgrade.Configs
{
    [CreateAssetMenu(menuName = "Configs/Upgrade/" + nameof(UpgradeConfig), fileName = nameof(UpgradeConfig))]
    public class UpgradeConfig : ScriptableObject
    {
        public IReadOnlyList<float> Multipliers => _multipliers;
        public float PriceTolerance => _priceTolerance;
        public int TargetAttempts => _targetAttempts;
        public int MaxCandidates => _maxCandidates;
        public int TargetsCount => _targetsCount;
        public float MaxChance => _maxChance;
        public float RollDuration => _rollDuration;
        public int RollTurns => _rollTurns;
        public float ResultHoldDuration => _resultHoldDuration;

        [SerializeField] private List<float> _multipliers = new List<float>() { 1.5f, 2f, 3f, 5f, 10f };
        [SerializeField, Range(0f, 0.5f)] private float _priceTolerance = 0.1f;
        [SerializeField, Min(1)] private int _targetAttempts = 20;
        [SerializeField, Min(1)] private int _maxCandidates = 30;
        [SerializeField, Min(1)] private int _targetsCount = 8;
        [SerializeField, Range(0.01f, 1f)] private float _maxChance = 0.8f;

        [Header("Roll animation")]
        [SerializeField, Min(0.1f)] private float _rollDuration = 3f;
        [SerializeField, Min(1)] private int _rollTurns = 4;
        [SerializeField, Min(0f)] private float _resultHoldDuration = 1.5f;
    }
}

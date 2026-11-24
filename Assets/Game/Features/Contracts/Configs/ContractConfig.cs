using Features.Skins.Data;
using UnityEngine;

namespace Features.Contracts.Configs
{
    [CreateAssetMenu(menuName = "Configs/Contracts/" + nameof(ContractConfig), fileName = nameof(ContractConfig))]
    public class ContractConfig : ScriptableObject
    {
        public int InputsCount => _inputsCount;
        public Rarity MinInputRarity => _minInputRarity;
        public Rarity MaxInputRarity => _maxInputRarity;
        public float MinInkCoverage => _minInkCoverage;
        public float ResultRevealDelay => _resultRevealDelay;

        [SerializeField, Min(1)] private int _inputsCount = 10;
        [SerializeField] private Rarity _minInputRarity = Rarity.ConsumerGrade;
        [SerializeField] private Rarity _maxInputRarity = Rarity.Classified;

        [Header("Signature")]
        [SerializeField, Range(0.001f, 1f)] private float _minInkCoverage = 0.04f;
        [SerializeField, Min(0f)] private float _resultRevealDelay = 0.4f;
    }
}

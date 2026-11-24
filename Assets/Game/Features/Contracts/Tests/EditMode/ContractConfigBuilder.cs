using Features.Contracts.Configs;
using Features.Skins.Data;
using UnityEditor;
using UnityEngine;

namespace Features.Contracts.Tests.EditMode
{
    public class ContractConfigBuilder
    {
        private int _inputsCount = 10;
        private Rarity _minInputRarity = Rarity.ConsumerGrade;
        private Rarity _maxInputRarity = Rarity.Classified;

        public ContractConfigBuilder WithInputsCount(int value)
        {
            _inputsCount = value;
            return this;
        }

        public ContractConfig Build()
        {
            var config = ScriptableObject.CreateInstance<ContractConfig>();
            var serializedObject = new SerializedObject(config);

            serializedObject.FindProperty("_inputsCount").intValue = _inputsCount;
            serializedObject.FindProperty("_minInputRarity").enumValueIndex = (int)_minInputRarity;
            serializedObject.FindProperty("_maxInputRarity").enumValueIndex = (int)_maxInputRarity;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            return config;
        }
    }
}

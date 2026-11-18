using System.Collections.Generic;
using Features.Upgrade.Configs;
using UnityEditor;
using UnityEngine;

namespace Features.Upgrade.Tests.EditMode
{
    public class UpgradeConfigBuilder
    {
        private List<float> _multipliers = new List<float>() { 1.5f, 2f, 3f, 5f, 10f };
        private float _returnToPlayer = 0.9f;
        private float _priceTolerance = 0.1f;
        private int _targetAttempts = 20;
        private int _maxCandidates = 30;

        public UpgradeConfigBuilder WithReturnToPlayer(float value)
        {
            _returnToPlayer = value;
            return this;
        }

        public UpgradeConfigBuilder WithPriceTolerance(float value)
        {
            _priceTolerance = value;
            return this;
        }

        public UpgradeConfigBuilder WithTargetAttempts(int value)
        {
            _targetAttempts = value;
            return this;
        }

        public UpgradeConfigBuilder WithMaxCandidates(int value)
        {
            _maxCandidates = value;
            return this;
        }

        public UpgradeConfig Build()
        {
            var config = ScriptableObject.CreateInstance<UpgradeConfig>();
            var serializedObject = new SerializedObject(config);

            var multipliers = serializedObject.FindProperty("_multipliers");
            multipliers.arraySize = _multipliers.Count;

            for (var index = 0; index < _multipliers.Count; index++)
            {
                multipliers.GetArrayElementAtIndex(index).floatValue = _multipliers[index];
            }

            serializedObject.FindProperty("_returnToPlayer").floatValue = _returnToPlayer;
            serializedObject.FindProperty("_priceTolerance").floatValue = _priceTolerance;
            serializedObject.FindProperty("_targetAttempts").intValue = _targetAttempts;
            serializedObject.FindProperty("_maxCandidates").intValue = _maxCandidates;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            return config;
        }
    }
}

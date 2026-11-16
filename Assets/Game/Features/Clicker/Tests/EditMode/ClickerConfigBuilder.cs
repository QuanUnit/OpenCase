using Features.Clicker.Configs;
using UnityEditor;
using UnityEngine;

namespace Features.Clicker.Tests.EditMode
{
    public class ClickerConfigBuilder
    {
        private float _rewardPerClick = 0.01f;
        private float _particlesPerCent = 4f;
        private int _minParticles = 3;
        private int _maxParticles = 12;

        public ClickerConfigBuilder WithRewardPerClick(float value)
        {
            _rewardPerClick = value;
            return this;
        }

        public ClickerConfigBuilder WithParticles(float perCent, int min, int max)
        {
            _particlesPerCent = perCent;
            _minParticles = min;
            _maxParticles = max;
            return this;
        }

        public ClickerConfig Build()
        {
            var config = ScriptableObject.CreateInstance<ClickerConfig>();
            var serializedObject = new SerializedObject(config);

            serializedObject.FindProperty("_rewardPerClick").floatValue = _rewardPerClick;
            serializedObject.FindProperty("_particlesPerCent").floatValue = _particlesPerCent;
            serializedObject.FindProperty("_minParticles").intValue = _minParticles;
            serializedObject.FindProperty("_maxParticles").intValue = _maxParticles;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            return config;
        }
    }
}

using Features.Skins.Configs;
using UnityEditor;
using UnityEngine;

namespace Features.Skins.Tests.EditMode
{
    public class SkinsGlobalSettingsConfigBuilder
    {
        private float _statTrakChance = 0f;
        private float _souvenirChance = 0f;
        private float _priceMultiplierByStatTrak = 1f;
        private float _priceMultiplierBySouvenir = 1f;
        private AnimationCurve _floatValueCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        private AnimationCurve _priceByFloatValueCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public SkinsGlobalSettingsConfigBuilder WithStatTrakChance(float value)
        {
            _statTrakChance = value;
            return this;
        }

        public SkinsGlobalSettingsConfigBuilder WithSouvenirChance(float value)
        {
            _souvenirChance = value;
            return this;
        }

        public SkinsGlobalSettingsConfigBuilder WithPriceMultiplierByStatTrak(float value)
        {
            _priceMultiplierByStatTrak = value;
            return this;
        }

        public SkinsGlobalSettingsConfigBuilder WithPriceMultiplierBySouvenir(float value)
        {
            _priceMultiplierBySouvenir = value;
            return this;
        }

        public SkinsGlobalSettingsConfigBuilder WithFloatValueCurve(AnimationCurve value)
        {
            _floatValueCurve = value;
            return this;
        }

        public SkinsGlobalSettingsConfigBuilder WithPriceByFloatValueCurve(AnimationCurve value)
        {
            _priceByFloatValueCurve = value;
            return this;
        }

        public SkinsGlobalSettingsConfig Build()
        {
            var config = ScriptableObject.CreateInstance<SkinsGlobalSettingsConfig>();
            var serializedObject = new SerializedObject(config);

            serializedObject.FindProperty("_statTrakChance").floatValue = _statTrakChance;
            serializedObject.FindProperty("_souvenirChance").floatValue = _souvenirChance;
            serializedObject.FindProperty("_priceMultiplierByStatTrak").floatValue = _priceMultiplierByStatTrak;
            serializedObject.FindProperty("_priceMultiplierBySouvenir").floatValue = _priceMultiplierBySouvenir;
            serializedObject.FindProperty("_floatValueCurve").animationCurveValue = _floatValueCurve;
            serializedObject.FindProperty("_priceByFloatValueCurve").animationCurveValue = _priceByFloatValueCurve;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            return config;
        }
    }
}

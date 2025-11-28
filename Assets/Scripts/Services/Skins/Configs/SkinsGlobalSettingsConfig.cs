using System.Collections.Generic;
using QCore.Tools.Collections;
using QCore.Tools.Mathematics;
using UnityEngine;

namespace Services.Skins.Configs
{
    [CreateAssetMenu(menuName = "Configs/Skins/" + nameof(SkinConfig), fileName = nameof(SkinConfig))]
    public class SkinsGlobalSettingsConfig : ScriptableObject
    {
        public AnimationCurve FloatValueCurve => _floatValueCurve;
        public AnimationCurve PriceByFloatValueCurve => _priceByFloatValueCurve;
        public float PriceMultiplierByStatTrak => _priceMultiplierByStatTrak;
        public float PriceMultiplierBySouvenir => _priceMultiplierBySouvenir;
        public float StatTrakChance => _statTrakChance;
        public float SouvenirChance => _souvenirChance;
        public SerializableKeyValuePairCollection<Wear, FloatRange> WearValuesMap => _wearValuesMap;
        
        [SerializeField] private AnimationCurve _floatValueCurve;
        [SerializeField] private AnimationCurve _priceByFloatValueCurve;
        [SerializeField] [Range(0f, 1f)] private float _statTrakChance;
        [SerializeField] [Range(0f, 1f)] private float _souvenirChance;
        [SerializeField] private float _priceMultiplierByStatTrak;
        [SerializeField] private float _priceMultiplierBySouvenir;
        [SerializeField] private SerializableKeyValuePairCollection<Wear, FloatRange> _wearValuesMap;
    }
}
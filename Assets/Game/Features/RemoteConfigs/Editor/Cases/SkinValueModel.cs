using Features.Skins.Configs;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class SkinValueModel
    {
        public readonly AnimationCurve FloatValueCurve;
        public readonly AnimationCurve PriceByFloatValueCurve;
        public readonly float StatTrakChance;
        public readonly float SouvenirChance;
        public readonly float StatTrakMultiplier;
        public readonly float SouvenirMultiplier;

        public SkinValueModel(SkinsGlobalSettingsConfig config)
            : this(config.FloatValueCurve, config.PriceByFloatValueCurve, config.StatTrakChance, config.SouvenirChance, config.PriceMultiplierByStatTrak, config.PriceMultiplierBySouvenir)
        {
        }

        public SkinValueModel(AnimationCurve floatValueCurve, AnimationCurve priceByFloatValueCurve, float statTrakChance, float souvenirChance, float statTrakMultiplier, float souvenirMultiplier)
        {
            FloatValueCurve = floatValueCurve;
            PriceByFloatValueCurve = priceByFloatValueCurve;
            StatTrakChance = statTrakChance;
            SouvenirChance = souvenirChance;
            StatTrakMultiplier = statTrakMultiplier;
            SouvenirMultiplier = souvenirMultiplier;
        }

        public int GetFingerprint()
        {
            var hash = 17;

            hash = Combine(hash, FloatValueCurve);
            hash = Combine(hash, PriceByFloatValueCurve);
            hash = Combine(hash, StatTrakChance.GetHashCode());
            hash = Combine(hash, SouvenirChance.GetHashCode());
            hash = Combine(hash, StatTrakMultiplier.GetHashCode());
            hash = Combine(hash, SouvenirMultiplier.GetHashCode());

            return hash;
        }

        private int Combine(int hash, AnimationCurve curve)
        {
            foreach (var key in curve.keys)
            {
                hash = Combine(hash, key.time.GetHashCode());
                hash = Combine(hash, key.value.GetHashCode());
                hash = Combine(hash, key.inTangent.GetHashCode());
                hash = Combine(hash, key.outTangent.GetHashCode());
                hash = Combine(hash, key.inWeight.GetHashCode());
                hash = Combine(hash, key.outWeight.GetHashCode());
                hash = Combine(hash, (int)key.weightedMode);
            }

            return hash;
        }

        private int Combine(int hash, int value)
        {
            unchecked
            {
                return hash * 31 + value;
            }
        }
    }
}

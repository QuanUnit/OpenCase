using QCore.Tools.Identifiers;
using Services.Skins.Configs;
using Tools.Mathematics;
using UnityEngine;
using Zenject;

namespace Services.Skins
{
    public class SkinFactory : ISkinFactory
    {
        private readonly SkinsGlobalSettingsConfig _globalConfig;

        public SkinFactory(SkinsGlobalSettingsConfig globalConfig)
        {
            _globalConfig = globalConfig;
        }
        
        public Skin CreateRandom(SkinConfig config)
        {
            bool isStatTrack = RandomStatTrak(config);
            bool isSouvenir = RandomSouvenir(config);
            
            float floatValue = Random.Range(0f, 1f);
            
            floatValue = _globalConfig.FloatValueCurve.Evaluate(floatValue);

            float price = CalculatePrice(config.Price, floatValue, isStatTrack, isSouvenir);
            QGuid id = QGuid.CreateNew();
            
            return new Skin(config.Id, id, floatValue, price, isStatTrack, isSouvenir);
        }

        private bool RandomStatTrak(SkinConfig config)
        {
            if (config.AvailableStatTrak == false) return false;

            return _globalConfig.StatTrakChance.IsProke();
        }
        
        private bool RandomSouvenir(SkinConfig config)
        {
            if (config.AvailableSouvenir == false) return false;

            return _globalConfig.SouvenirChance.IsProke();
        }
        
        private float CalculatePrice(PriceRange priceRange, float floatValue, bool isStatTrak, bool isSouvenir)
        {
            float priceT = _globalConfig.PriceByFloatValueCurve.Evaluate(floatValue);
            float price = priceRange.GetInRange(priceT);

            if (isStatTrak)
            {
                price *= _globalConfig.PriceMultiplierByStatTrak;
            }

            if (isSouvenir)
            {
                price *= _globalConfig.PriceMultiplierBySouvenir;
            }

            return price;
        }
    }
}
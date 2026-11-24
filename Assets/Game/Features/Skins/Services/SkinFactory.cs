using QCore.Tools.Identifiers;
using Features.Skins.Configs;
using Shared.Tools.Mathematics;
using UnityEngine;
using Zenject;
using Features.Skins.Data;

namespace Features.Skins.Services
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
            var isStatTrack = RandomStatTrak(config);
            var isSouvenir = RandomSouvenir(config);
            
            var floatValue = Random.Range(0f, 1f);
            
            floatValue = _globalConfig.FloatValueCurve.Evaluate(floatValue);

            var price = CalculatePrice(config.Price, floatValue, isStatTrack, isSouvenir);
            var id = QGuid.CreateNew();
            
            return new Skin(config.Id, id, floatValue, price, isStatTrack, isSouvenir);
        }

        public Skin Create(SkinConfig config, float floatValue, bool isStatTrak)
        {
            var price = CalculatePrice(config.Price, floatValue, isStatTrak, false);
            var id = QGuid.CreateNew();

            return new Skin(config.Id, id, floatValue, price, isStatTrak, false);
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
            var priceT = _globalConfig.PriceByFloatValueCurve.Evaluate(floatValue);
            var price = priceRange.GetInRange(priceT);

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
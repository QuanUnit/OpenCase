using System;
using System.Collections.Generic;
using System.Linq;
using Features.Inventory.Services;
using Features.Skins.Configs;
using Features.Skins.Data;
using Features.Skins.Services;
using Features.Upgrade.Configs;
using Features.Upgrade.Data;
using Shared.Tools.Collections;

namespace Features.Upgrade.Services
{
    public class UpgradeService : IUpgradeService
    {
        private readonly UpgradeConfig _config;
        private readonly ISkinsDatabaseService _skinsDatabaseService;
        private readonly ISkinFactory _skinFactory;
        private readonly IInventoryService _inventoryService;
        private readonly IChanceRoller _chanceRoller;

        public IReadOnlyList<float> Multipliers => _config.Multipliers;

        public UpgradeService(UpgradeConfig config, ISkinsDatabaseService skinsDatabaseService, ISkinFactory skinFactory, IInventoryService inventoryService, IChanceRoller chanceRoller)
        {
            _config = config;
            _skinsDatabaseService = skinsDatabaseService;
            _skinFactory = skinFactory;
            _inventoryService = inventoryService;
            _chanceRoller = chanceRoller;
        }

        public bool TryCreateOffer(Skin source, float multiplier, out UpgradeOffer offer)
        {
            offer = null;

            if (source.Price <= 0f)
            {
                return false;
            }

            var targetPrice = source.Price * multiplier;
            var minPrice = Math.Max(targetPrice * (1f - _config.PriceTolerance), source.Price);
            var maxPrice = targetPrice * (1f + _config.PriceTolerance);

            var candidates = GetCandidates(minPrice, maxPrice);

            foreach (var candidate in candidates)
            {
                var target = FindClosestTarget(candidate, source.Price, minPrice, maxPrice, targetPrice);

                if (target != null)
                {
                    var chance = source.Price / target.Price * _config.ReturnToPlayer;
                    offer = new UpgradeOffer(source, target, multiplier, chance);

                    return true;
                }
            }

            return false;
        }

        public bool Upgrade(UpgradeOffer offer)
        {
            if (_inventoryService.Remove(offer.Source) == false)
            {
                return false;
            }

            var isSuccess = _chanceRoller.Roll(offer.Chance);

            if (isSuccess == true)
            {
                _inventoryService.Add(offer.Target);
            }

            return isSuccess;
        }

        private List<SkinConfig> GetCandidates(float minPrice, float maxPrice)
        {
            var candidates = _skinsDatabaseService.AllSkinsConfigs
                .Where(config => config.Price.MaxPrice >= minPrice && config.Price.MinPrice <= maxPrice)
                .ToList();

            candidates.Shuffle();

            if (candidates.Count > _config.MaxCandidates)
            {
                candidates.RemoveRange(_config.MaxCandidates, candidates.Count - _config.MaxCandidates);
            }

            return candidates;
        }

        private Skin FindClosestTarget(SkinConfig candidate, float sourcePrice, float minPrice, float maxPrice, float targetPrice)
        {
            Skin closest = null;
            var closestDistance = float.MaxValue;

            for (var attempt = 0; attempt < _config.TargetAttempts; attempt++)
            {
                var skin = _skinFactory.CreateRandom(candidate);

                if (skin.Price <= sourcePrice || skin.Price < minPrice || skin.Price > maxPrice)
                {
                    continue;
                }

                var distance = Math.Abs(skin.Price - targetPrice);

                if (distance < closestDistance)
                {
                    closest = skin;
                    closestDistance = distance;
                }
            }

            return closest;
        }
    }
}

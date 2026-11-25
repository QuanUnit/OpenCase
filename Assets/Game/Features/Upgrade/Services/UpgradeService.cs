using System;
using System.Collections.Generic;
using System.Linq;
using Features.Inventory.Services;
using Features.Skins.Configs;
using Features.Skins.Data;
using Features.Skins.Services;
using Features.Upgrade.Configs;
using Features.Upgrade.Data;
using Features.Wallet.Services;
using Shared.Tools.Collections;
using Shared.Tools.Mathematics;

namespace Features.Upgrade.Services
{
    public class UpgradeService : IUpgradeService
    {
        private readonly UpgradeConfig _config;
        private readonly ISkinsDatabaseService _skinsDatabaseService;
        private readonly ISkinFactory _skinFactory;
        private readonly IInventoryService _inventoryService;
        private readonly IChanceRoller _chanceRoller;
        private readonly SoftCurrencyWallet _wallet;

        public IReadOnlyList<float> Multipliers => _config.Multipliers;

        public UpgradeService(UpgradeConfig config, ISkinsDatabaseService skinsDatabaseService, ISkinFactory skinFactory, IInventoryService inventoryService, IChanceRoller chanceRoller, SoftCurrencyWallet wallet)
        {
            _config = config;
            _skinsDatabaseService = skinsDatabaseService;
            _skinFactory = skinFactory;
            _inventoryService = inventoryService;
            _chanceRoller = chanceRoller;
            _wallet = wallet;
        }

        public IReadOnlyList<Skin> CreateTargets(Skin source, float multiplier)
        {
            var targets = new List<Skin>();

            if (source.Price <= 0f)
            {
                return targets;
            }

            var targetPrice = source.Price * multiplier;
            var minPrice = Math.Max(targetPrice * (1f - _config.PriceTolerance), source.Price);
            var maxPrice = targetPrice * (1f + _config.PriceTolerance);

            var candidates = GetCandidates(minPrice, maxPrice);

            foreach (var candidate in candidates)
            {
                if (targets.Count >= _config.TargetsCount)
                {
                    break;
                }

                var target = FindClosestTarget(candidate, source.Price, minPrice, maxPrice, targetPrice);

                if (target != null)
                {
                    targets.Add(target);
                }
            }

            targets.Sort((first, second) => first.Price.CompareTo(second.Price));

            return targets;
        }

        public float GetChance(Skin source, Skin target, float extra)
        {
            var chance = (source.Price + extra) / target.Price * _config.ReturnToPlayer;

            return Math.Min(chance, _config.MaxChance);
        }

        public float GetMaxExtra(Skin source, Skin target)
        {
            var extraForMaxChance = _config.MaxChance * target.Price / _config.ReturnToPlayer - source.Price;
            var balance = Math.Min(MoneyMath.RoundToMoney(_wallet.Currency), _wallet.Currency);
            var maxExtra = Math.Min(balance, MoneyMath.FloorToMoney(extraForMaxChance));

            return Math.Max(maxExtra, 0f);
        }

        public UpgradeOffer CreateOffer(Skin source, Skin target, float extra)
        {
            var maxExtra = GetMaxExtra(source, target);
            var clampedExtra = Math.Clamp(MoneyMath.RoundToMoney(extra), 0f, maxExtra);
            var chance = GetChance(source, target, clampedExtra);

            return new UpgradeOffer(source, target, clampedExtra, chance);
        }

        public UpgradeResult Upgrade(UpgradeOffer offer)
        {
            if (_inventoryService.TryGet(offer.Source.Id, out _) == false)
            {
                return UpgradeResult.Rejected;
            }

            if (offer.Extra > 0f && _wallet.TrySpend(offer.Extra) == false)
            {
                return UpgradeResult.Rejected;
            }

            _inventoryService.Remove(offer.Source);

            var isSuccess = _chanceRoller.Roll(offer.Chance);

            if (isSuccess == true)
            {
                _inventoryService.Add(offer.Target);

                return UpgradeResult.Success;
            }

            return UpgradeResult.Lost;
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

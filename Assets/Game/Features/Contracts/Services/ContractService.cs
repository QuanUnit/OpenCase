using System;
using System.Collections.Generic;
using System.Linq;
using Features.Contracts.Configs;
using Features.Contracts.Data;
using Features.Inventory.Services;
using Features.Skins.Configs;
using Features.Skins.Data;
using Features.Skins.Services;

namespace Features.Contracts.Services
{
    public class ContractService : IContractService
    {
        private readonly ContractConfig _config;
        private readonly ISkinsDatabaseService _skinsDatabaseService;
        private readonly ISkinFactory _skinFactory;
        private readonly IInventoryService _inventoryService;
        private readonly IContractRandom _contractRandom;

        private static readonly IReadOnlyList<SkinConfig> EmptyCandidates = Array.Empty<SkinConfig>();

        private Dictionary<(SkinCollectionId Collection, Rarity Rarity), List<SkinConfig>> _index;

        public int InputsCount => _config.InputsCount;

        public ContractService(ContractConfig config, ISkinsDatabaseService skinsDatabaseService, ISkinFactory skinFactory, IInventoryService inventoryService, IContractRandom contractRandom)
        {
            _config = config;
            _skinsDatabaseService = skinsDatabaseService;
            _skinFactory = skinFactory;
            _inventoryService = inventoryService;
            _contractRandom = contractRandom;
        }

        public bool IsEligible(Skin skin)
        {
            if (skin.IsSouvenir == true)
            {
                return false;
            }

            var config = _skinsDatabaseService.GetSkinConfigById(skin.ConfigId);

            if (config.Rarity < _config.MinInputRarity || config.Rarity > _config.MaxInputRarity)
            {
                return false;
            }

            if (config.Collection.IsEmpty == true)
            {
                return false;
            }

            return GetCandidates(config.Collection, config.Rarity + 1).Count > 0;
        }

        public bool CanAdd(IReadOnlyList<Skin> inputs, Skin candidate)
        {
            if (inputs.Count >= _config.InputsCount)
            {
                return false;
            }

            if (IsEligible(candidate) == false)
            {
                return false;
            }

            if (inputs.Any(input => input.Id == candidate.Id) == true)
            {
                return false;
            }

            if (inputs.Count == 0)
            {
                return true;
            }

            var first = inputs[0];

            if (candidate.IsStatTrak != first.IsStatTrak)
            {
                return false;
            }

            var firstConfig = _skinsDatabaseService.GetSkinConfigById(first.ConfigId);
            var candidateConfig = _skinsDatabaseService.GetSkinConfigById(candidate.ConfigId);

            return candidateConfig.Rarity == firstConfig.Rarity;
        }

        public IReadOnlyList<ContractOutcome> GetOutcomes(IReadOnlyList<Skin> inputs)
        {
            var outcomes = new List<ContractOutcome>();

            if (inputs.Count == 0)
            {
                return outcomes;
            }

            var chances = new Dictionary<SkinConfig, float>();
            var order = new List<SkinConfig>();

            foreach (var input in inputs)
            {
                var config = _skinsDatabaseService.GetSkinConfigById(input.ConfigId);
                var candidates = GetCandidates(config.Collection, config.Rarity + 1);
                var chance = 1f / (inputs.Count * candidates.Count);

                foreach (var candidate in candidates)
                {
                    if (chances.ContainsKey(candidate) == true)
                    {
                        chances[candidate] += chance;
                    }
                    else
                    {
                        chances.Add(candidate, chance);
                        order.Add(candidate);
                    }
                }
            }

            foreach (var config in order)
            {
                outcomes.Add(new ContractOutcome(config, chances[config]));
            }

            return outcomes.OrderByDescending(outcome => outcome.Chance).ToList();
        }

        public bool TryExecute(IReadOnlyList<Skin> inputs, out Skin result)
        {
            result = null;

            if (inputs.Count != _config.InputsCount)
            {
                return false;
            }

            var accepted = new List<Skin>();

            foreach (var input in inputs)
            {
                if (CanAdd(accepted, input) == false)
                {
                    return false;
                }

                accepted.Add(input);
            }

            foreach (var input in inputs)
            {
                if (_inventoryService.TryGet(input.Id, out _) == false)
                {
                    return false;
                }
            }

            var outcomes = GetOutcomes(inputs);
            var picked = PickOutcome(outcomes);
            var floatValue = inputs.Average(input => input.FloatValue);

            result = _skinFactory.Create(picked.Config, floatValue, inputs[0].IsStatTrak);

            var snapshot = inputs.ToList();

            foreach (var input in snapshot)
            {
                _inventoryService.Remove(input);
            }

            _inventoryService.Add(result);

            return true;
        }

        private ContractOutcome PickOutcome(IReadOnlyList<ContractOutcome> outcomes)
        {
            var roll = _contractRandom.Next();
            var cumulative = 0f;

            foreach (var outcome in outcomes)
            {
                cumulative += outcome.Chance;

                if (roll < cumulative)
                {
                    return outcome;
                }
            }

            return outcomes[outcomes.Count - 1];
        }

        private IReadOnlyList<SkinConfig> GetCandidates(SkinCollectionId collection, Rarity rarity)
        {
            EnsureIndex();

            if (_index.TryGetValue((collection, rarity), out var candidates) == true)
            {
                return candidates;
            }

            return EmptyCandidates;
        }

        private void EnsureIndex()
        {
            if (_index != null)
            {
                return;
            }

            _index = new Dictionary<(SkinCollectionId Collection, Rarity Rarity), List<SkinConfig>>();

            foreach (var config in _skinsDatabaseService.AllSkinsConfigs)
            {
                if (config.Collection.IsEmpty == true)
                {
                    continue;
                }

                var key = (config.Collection, config.Rarity);

                if (_index.TryGetValue(key, out var list) == false)
                {
                    list = new List<SkinConfig>();
                    _index.Add(key, list);
                }

                list.Add(config);
            }
        }
    }
}

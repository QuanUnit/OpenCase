using System.Collections.Generic;
using Features.Contracts.Data;
using Features.Skins.Data;

namespace Features.Contracts.Services
{
    public interface IContractService
    {
        public int InputsCount { get; }

        public bool IsEligible(Skin skin);
        public bool CanAdd(IReadOnlyList<Skin> inputs, Skin candidate);
        public IReadOnlyList<ContractOutcome> GetOutcomes(IReadOnlyList<Skin> inputs);
        public bool TryExecute(IReadOnlyList<Skin> inputs, out Skin result);
    }
}

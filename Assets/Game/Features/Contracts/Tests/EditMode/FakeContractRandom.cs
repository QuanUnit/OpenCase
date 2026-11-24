using Features.Contracts.Services;

namespace Features.Contracts.Tests.EditMode
{
    public class FakeContractRandom : IContractRandom
    {
        public float Value { get; set; }

        public float Next()
        {
            return Value;
        }
    }
}

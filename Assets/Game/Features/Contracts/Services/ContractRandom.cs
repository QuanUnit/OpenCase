using UnityEngine;

namespace Features.Contracts.Services
{
    public class ContractRandom : IContractRandom
    {
        public float Next()
        {
            var value = Random.value;

            while (value >= 1f)
            {
                value = Random.value;
            }

            return value;
        }
    }
}

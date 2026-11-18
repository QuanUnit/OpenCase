using UnityEngine;

namespace Features.Upgrade.Services
{
    public class ChanceRoller : IChanceRoller
    {
        public bool Roll(float chance)
        {
            return Random.value < chance;
        }
    }
}

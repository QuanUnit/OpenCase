using Features.Upgrade.Services;

namespace Features.Upgrade.Tests.EditMode
{
    public class FakeChanceRoller : IChanceRoller
    {
        public bool Result { get; set; }
        public float LastChance { get; private set; }
        public int RollsCount { get; private set; }

        public bool Roll(float chance)
        {
            RollsCount++;
            LastChance = chance;

            return Result;
        }
    }
}

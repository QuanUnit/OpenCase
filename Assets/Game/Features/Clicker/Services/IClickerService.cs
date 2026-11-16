namespace Features.Clicker.Services
{
    public interface IClickerService
    {
        public float HoldInterval { get; }

        public float Earn();
        public int GetParticlesCount(float amount);
    }
}

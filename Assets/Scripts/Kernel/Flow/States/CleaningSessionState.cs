using QCore.StateMachine.States;
using Services.Skins;
using Zenject;

namespace Kernel.Flow.States
{
    public class CleaningSessionState : State
    {
        [Inject] private ISkinsDatabaseService _skinsDatabaseService;
        
        public override void Enter()
        {
            DisposeServices();
            ReleaseAssets();
        }

        private void DisposeServices()
        {
            
        }

        private void ReleaseAssets()
        {
            _skinsDatabaseService.ReleaseAllSkins();
        }
    }
}
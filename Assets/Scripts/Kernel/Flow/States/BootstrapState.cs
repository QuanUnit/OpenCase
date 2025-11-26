using QCore.StateMachine.States;
using QCore.UISystem.Services;
using Zenject;

namespace Kernel.Flow.States
{
    public class BootstrapState : State
    {
        [Inject] private IUIPanelsService _panelsService;
        
        public override void Enter()
        {
            InitServices();
            
            StateMachine.EnterState<LoadingSaveState>();
        }

        private void InitServices()
        {
            _panelsService.Init();
        }
    }
}
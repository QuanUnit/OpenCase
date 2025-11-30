using QCore.StateMachine.States;
using QCore.UISystem.Services;
using Services.Cases;
using Services.Skins;
using Zenject;

namespace Kernel.Flow.States
{
    public class BootstrapState : State
    {
        [Inject] private IUIPanelsService _panelsService;
        [Inject] private ISkinsDatabaseService _skinsDatabaseService;
        [Inject] private ISkinsUIContextDataService _skinsUIContextDataService;
        [Inject] private ICasesDatabaseService _casesDatabaseService;
        
        public override void Enter()
        {
            InitServices();
            
            StateMachine.EnterState<LoadingSaveState>();
        }

        private void InitServices()
        {
            _panelsService.Init();
            _skinsDatabaseService.Init();
            _skinsUIContextDataService.Init();
            _casesDatabaseService.Init();
        }
    }
}
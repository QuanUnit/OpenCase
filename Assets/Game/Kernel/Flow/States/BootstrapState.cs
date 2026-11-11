using System.Collections.Generic;
using QCore.StateMachine.States;
using QCore.UISystem.Services;
using Shared.Lifecycle;

namespace Kernel.Flow.States
{
    public class BootstrapState : State
    {
        private readonly IUIPanelsService _panelsService;
        private readonly List<IInitializableService> _services;

        public BootstrapState(IUIPanelsService panelsService, List<IInitializableService> services)
        {
            _panelsService = panelsService;
            _services = services;
        }

        public override void Enter()
        {
            InitServices();

            StateMachine.EnterState<SettingPersistentWithoutSaveState>();
        }

        public void InitServices()
        {
            _panelsService.Init();

            foreach (var service in _services)
            {
                service.Init();
            }
        }
    }
}

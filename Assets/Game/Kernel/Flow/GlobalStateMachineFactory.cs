using Kernel.Flow.States;
using QCore.StateMachine.States;
using Zenject;

namespace Kernel.Flow
{
    public class GlobalStateMachineFactory
    {
        private readonly DiContainer _container;

        public GlobalStateMachineFactory(DiContainer container)
        {
            _container = container;
        }
        
        public GlobalStateMachine Create()
        {
            var stateMachine = new GlobalStateMachine();
            
            stateMachine.AddState(CreateState<BootstrapState>());
            stateMachine.AddState(CreateState<LoadingSaveState>());
            stateMachine.AddState(CreateState<SettingPersistentWithoutSaveState>());
            stateMachine.AddState(CreateState<LoadingAssetsState>());
            stateMachine.AddState(CreateState<OpeningSessionState>());
            stateMachine.AddState(CreateState<PlayingSessionState>());
            stateMachine.AddState(CreateState<CleaningSessionState>());

            return stateMachine;
        }

        private T CreateState<T>() where T : IExitableState
        {
            var result = _container.Instantiate<T>();
            return result;
        }
    }
}
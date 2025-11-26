using Kernel.Flow.States;
using QCore.StateMachine.States;
using Zenject;

namespace Kernel.Flow
{
    public class GlobalStateMachineFactory
    {
        [Inject] private DiContainer _container;
        
        public GlobalStateMachine Create()
        {
            GlobalStateMachine stateMachine = new GlobalStateMachine();
            
            stateMachine.AddState(CreateState<BootstrapState>());
            stateMachine.AddState(CreateState<LoadingSaveState>());
            stateMachine.AddState(CreateState<LoadingAssetsState>());
            stateMachine.AddState(CreateState<OpeningSessionState>());
            stateMachine.AddState(CreateState<PlayingSessionState>());
            stateMachine.AddState(CreateState<CleaningSessionState>());

            return stateMachine;
        }

        private T CreateState<T>() where T : IExitableState
        {
            T result = _container.Instantiate<T>();
            return result;
        }
    }
}
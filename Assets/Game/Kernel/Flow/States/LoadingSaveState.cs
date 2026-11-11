using QCore.StateMachine.States;

namespace Kernel.Flow.States
{
    public class LoadingSaveState : State
    {
        public override void Enter()
        {
            StateMachine.EnterState<LoadingAssetsState>();
        }
    }
}
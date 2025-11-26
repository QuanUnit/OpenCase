using Cysharp.Threading.Tasks;
using QCore.StateMachine.States;

namespace Kernel.Flow.States
{
    public class LoadingAssetsState : State
    {
        public override async void Enter()
        {
            await LoadAssets();
            
            StateMachine.EnterState<OpeningSessionState>();
        }

        public async UniTask LoadAssets()
        {
            
        }
    }
}
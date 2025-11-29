using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using QCore.StateMachine.States;
using Services.Skins;
using Zenject;

namespace Kernel.Flow.States
{
    public class LoadingAssetsState : State
    {
        [Inject] private ISkinsDatabaseService _skinsDatabaseService;
        
        public override async void Enter()
        {
            await LoadAssets();
            
            StateMachine.EnterState<OpeningSessionState>();
        }

        public async UniTask LoadAssets()
        {
            List<UniTask> tasks = new List<UniTask>()
            {
                _skinsDatabaseService.LoadAllSkins(),
            };

            await UniTask.WhenAll(tasks);
        }
    }
}
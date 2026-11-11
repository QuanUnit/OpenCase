using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using QCore.StateMachine.States;
using Shared.Lifecycle;

namespace Kernel.Flow.States
{
    public class LoadingAssetsState : State
    {
        private readonly List<IAssetsLoader> _loaders;

        public LoadingAssetsState(List<IAssetsLoader> loaders)
        {
            _loaders = loaders;
        }

        public override async void Enter()
        {
            await LoadAssets();

            StateMachine.EnterState<OpeningSessionState>();
        }

        public async UniTask LoadAssets()
        {
            var tasks = new List<UniTask>(_loaders.Count);

            foreach (var loader in _loaders)
            {
                tasks.Add(loader.LoadAssets());
            }

            await UniTask.WhenAll(tasks);
        }
    }
}

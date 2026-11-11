using System.Collections.Generic;
using QCore.StateMachine.States;
using Shared.Lifecycle;

namespace Kernel.Flow.States
{
    public class CleaningSessionState : State
    {
        private readonly List<IAssetsLoader> _loaders;

        public CleaningSessionState(List<IAssetsLoader> loaders)
        {
            _loaders = loaders;
        }

        public override void Enter()
        {
            foreach (var loader in _loaders)
            {
                loader.ReleaseAssets();
            }
        }
    }
}

using Cysharp.Threading.Tasks;
using GameHub.Root.Panels;
using Kernel.Configs;
using QCore.StateMachine.States;
using QCore.UISystem.Services;
using UnityEngine.AddressableAssets;
using Zenject;

namespace Kernel.Flow.States
{
    public class OpeningSessionState : State
    {
        [Inject] private ProjectKernelConfig _projectKernelConfig;
        [Inject] private IUIPanelsService _panelsService;
        
        public override async void Enter()
        {
            await LoadScene();
            OpenPersistentUI();
            
            StateMachine.EnterState<PlayingSessionState>();
        }

        private async UniTask LoadScene()
        {
            await Addressables.LoadSceneAsync(_projectKernelConfig.MainSceneName);
        }

        private void OpenPersistentUI()
        {
            _panelsService.Open<GameHubPanel>();
        }
    }
}
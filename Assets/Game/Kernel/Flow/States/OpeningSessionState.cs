using Cysharp.Threading.Tasks;
using Features.GameHub.UI.Panels;
using Kernel.Configs;
using QCore.StateMachine.States;
using QCore.UISystem.Services;
using UnityEngine.AddressableAssets;

namespace Kernel.Flow.States
{
    public class OpeningSessionState : State
    {
        private readonly ProjectKernelConfig _projectKernelConfig;
        private readonly IUIPanelsService _panelsService;

        public OpeningSessionState(ProjectKernelConfig projectKernelConfig, IUIPanelsService panelsService)
        {
            _projectKernelConfig = projectKernelConfig;
            _panelsService = panelsService;
        }
        
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
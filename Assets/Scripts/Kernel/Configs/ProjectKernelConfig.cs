using UnityEngine;

namespace Kernel.Configs
{
    [CreateAssetMenu(menuName = "Configs/Kernel/" + nameof(ProjectKernelConfig), fileName = nameof(ProjectKernelConfig))]
    public class ProjectKernelConfig : ScriptableObject
    {
        public string MainSceneName => _mainSceneName;

        [SerializeField] private string _mainSceneName;
    }
}
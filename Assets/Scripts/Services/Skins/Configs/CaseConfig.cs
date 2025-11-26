using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using QCore.AddressablesWrapper.Containers;
using QCore.Tools.Configs;
using UnityEngine;

namespace Services.Skins.Configs
{
    [CreateAssetMenu(menuName = "Configs/Skins/" + nameof(CaseConfig), fileName = nameof(CaseConfig))]
    public class CaseConfig : IdentifiedScriptableObject
    {
        public UIContextData ContextData => _contextData;
        public string Name => _name;
        public IReadOnlyList<SkinConfig> SkinConfigs => _skinConfigs;
        
        [SerializeField] private string _name;
        [SerializeField] private UIContextData _contextData;
        [SerializeField] private List<SkinConfig> _skinConfigs;

        public async UniTask LoadAssets(CancellationToken cancellationToken = default)
        {
            List<UniTask> tasks = new List<UniTask>(_skinConfigs.Count);

            foreach (var skin in _skinConfigs)
            {
                UniTask task = skin.LoadAssets(cancellationToken);
                tasks.Add(task);
            }

            await UniTask.WhenAll(tasks);
        }

        public void ReleaseAssets()
        {
            foreach (var skin in _skinConfigs)
            {
                skin.ReleaseAssets();
            }
        }
        
        [Serializable]
        public struct UIContextData
        {
            public AddressableLoadContainerSprite SpriteSource;
        }
    }
}
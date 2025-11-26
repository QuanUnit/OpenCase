using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using QCore.AddressablesWrapper.Containers;
using QCore.Tools.Configs;
using UnityEngine;

namespace Services.Skins.Configs
{
    [CreateAssetMenu(menuName = "Configs/Skins/" + nameof(SkinConfig), fileName = nameof(SkinConfig))]
    public class SkinConfig : IdentifiedScriptableObject
    {
        public UIContextData ContextData => _contextData;
        
        [SerializeField] private UIContextData _contextData;

        public async UniTask LoadAssets(CancellationToken cancellationToken = default)
        {
            await _contextData.SpriteSource.Load(cancellationToken);
        }

        public void ReleaseAssets()
        {
            _contextData.SpriteSource.Release();
        }
        
        [Serializable]
        public struct UIContextData
        {
            public AddressableLoadContainerSprite SpriteSource;
        }
    }
}
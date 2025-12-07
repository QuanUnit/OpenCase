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
        public string SkinStringId;
        public UIContextData ContextData;
        public Rarity Rarity;
        public PriceRange Price;
        public bool AvailableStatTrak;
        public bool AvailableSouvenir;

        public async UniTask LoadAssets(CancellationToken cancellationToken = default)
        {
            await ContextData.SpriteSource.Load(cancellationToken);
        }

        public void ReleaseAssets()
        {
            ContextData.SpriteSource.Release();
        }
        
        [Serializable]
        public struct UIContextData
        {
            public AddressableLoadContainerSprite SpriteSource;
        }
    }
}
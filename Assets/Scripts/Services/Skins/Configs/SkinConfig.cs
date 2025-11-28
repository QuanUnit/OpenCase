using System;
using System.ComponentModel;
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
        public Rarity Rarity => _rarity;
        public PriceRange Price => _price;
        public bool AvailableStatTrak => _availableStatTrak;
        public bool AvailableSouvenir => _availableSouvenir;
        
        [SerializeField] private UIContextData _contextData;
        [SerializeField] private Rarity _rarity;
        [SerializeField] private PriceRange _price;
        [SerializeField] private bool _availableStatTrak;
        [SerializeField] private bool _availableSouvenir;

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
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using QCore.AddressablesWrapper.Containers;
using QCore.Tools.Collections;
using QCore.Tools.Configs;
using Services.Skins.Configs;
using UnityEngine;

namespace Services.Cases.Configs
{
    [CreateAssetMenu(menuName = "Configs/Cases/" + nameof(CaseConfig), fileName = nameof(CaseConfig))]
    public class CaseConfig : IdentifiedScriptableObject
    {
        public UIContextData ContextData => _contextData;
        public string Name => _name;
        public float Price => _price;
        public WeightedMap<SkinConfig> SkinsMapEntries => _skinsMap;
        
        [SerializeField] private string _name;
        [SerializeField] private float _price;
        [SerializeField] private UIContextData _contextData;
        [SerializeField] private WeightedMap<SkinConfig> _skinsMap;

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
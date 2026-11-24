using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using QCore.AddressablesWrapper.Containers;
using QCore.Tools.Collections;
using Features.Cases.Data;
using Features.Skins.Configs;
using Shared.Identifiers;
using UnityEngine;

namespace Features.Cases.Configs
{
    [CreateAssetMenu(menuName = "Configs/Cases/" + nameof(CaseConfig), fileName = nameof(CaseConfig))]
    public class CaseConfig : ScriptableObject
    {
        [EntityIdDefinition] public CaseId Id = new CaseId();
        public string Name;
        public float Price;
        public WeightedMap<SkinConfig> SkinsMapEntries;
        public UIContextData ContextData;
        
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
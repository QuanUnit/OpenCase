using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using QCore.AddressablesWrapper.Containers;
using Shared.Identifiers;
using UnityEngine;
using Features.Skins.Data;

namespace Features.Skins.Configs
{
    [CreateAssetMenu(menuName = "Configs/Skins/" + nameof(SkinConfig), fileName = nameof(SkinConfig))]
    public class SkinConfig : ScriptableObject
    {
        private const char IdSeparator = '_';
        private const char NameSeparator = ' ';

        [EntityIdDefinition] public SkinId Id = new SkinId();
        public string Name;
        public UIContextData ContextData;
        public Rarity Rarity;
        public SkinCollectionId Collection = new SkinCollectionId();
        public PriceRange Price;
        public bool AvailableStatTrak;
        public bool AvailableSouvenir;

        public string WeaponName => GetWeaponName();

        public async UniTask LoadAssets(CancellationToken cancellationToken = default)
        {
            await ContextData.SpriteSource.Load(cancellationToken);
        }

        public void ReleaseAssets()
        {
            ContextData.SpriteSource.Release();
        }

        private string GetWeaponName()
        {
            var id = Id.Value;
            var nameSuffix = IdSeparator + Name.Replace(NameSeparator, IdSeparator);

            if (id.EndsWith(nameSuffix, StringComparison.Ordinal) == false || id.Length <= nameSuffix.Length)
            {
                return string.Empty;
            }

            return id
                .Substring(0, id.Length - nameSuffix.Length)
                .Replace(IdSeparator, NameSeparator);
        }
        
        [Serializable]
        public struct UIContextData
        {
            public AddressableLoadContainerSprite SpriteSource;
        }
    }
}
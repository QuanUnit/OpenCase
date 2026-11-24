using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using QCore.AddressablesWrapper.Containers;
using QCore.Tools.Configs;
using UnityEngine;
using Features.Skins.Data;

namespace Features.Skins.Configs
{
    [CreateAssetMenu(menuName = "Configs/Skins/" + nameof(SkinConfig), fileName = nameof(SkinConfig))]
    public class SkinConfig : IdentifiedScriptableObject
    {
        private const char IdSeparator = '_';
        private const char NameSeparator = ' ';

        public string SkinStringId;
        public string Name;
        public UIContextData ContextData;
        public Rarity Rarity;
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
            var nameSuffix = IdSeparator + Name.Replace(NameSeparator, IdSeparator);

            if (SkinStringId.EndsWith(nameSuffix, StringComparison.Ordinal) == false || SkinStringId.Length <= nameSuffix.Length)
            {
                return string.Empty;
            }

            return SkinStringId
                .Substring(0, SkinStringId.Length - nameSuffix.Length)
                .Replace(IdSeparator, NameSeparator);
        }
        
        [Serializable]
        public struct UIContextData
        {
            public AddressableLoadContainerSprite SpriteSource;
        }
    }
}
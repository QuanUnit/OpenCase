using System;
using System.Collections.Generic;
using Features.Skins.Data;
using Shared.Identifiers;
using UnityEngine;

namespace Features.Skins.Configs
{
    [CreateAssetMenu(menuName = "Configs/Skins/" + nameof(SkinCollectionsConfig), fileName = nameof(SkinCollectionsConfig))]
    public class SkinCollectionsConfig : ScriptableObject
    {
        public List<Entry> Collections = new List<Entry>();

        [Serializable]
        public class Entry
        {
            [EntityIdDefinition] public SkinCollectionId Id = new SkinCollectionId();
            public string Name;
        }
    }
}

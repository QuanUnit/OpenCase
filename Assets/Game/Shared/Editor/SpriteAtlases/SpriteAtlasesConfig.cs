using System.Collections.Generic;
using UnityEngine;

namespace Shared.Editor.SpriteAtlases
{
    [CreateAssetMenu(menuName = "Configs/Editor/" + nameof(SpriteAtlasesConfig), fileName = nameof(SpriteAtlasesConfig))]
    public class SpriteAtlasesConfig : ScriptableObject
    {
        public IReadOnlyList<SpriteAtlasRule> Rules => _rules;

        [SerializeField] private List<SpriteAtlasRule> _rules = new List<SpriteAtlasRule>();
    }
}

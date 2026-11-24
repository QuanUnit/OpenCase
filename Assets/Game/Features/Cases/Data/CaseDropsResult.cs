using System.Collections.Generic;
using Features.Skins.Data;
using Shared.Tools.Mathematics;

namespace Features.Cases.Data
{
    public class CaseDropsResult
    {
        public int Count => _drops.Count;
        public float SoldTotal { get; private set; }

        private readonly List<Drop> _drops = new List<Drop>();

        public void Start(IReadOnlyList<Skin> skins)
        {
            Clear();

            foreach (var skin in skins)
            {
                _drops.Add(new Drop(skin));
            }
        }

        public void Clear()
        {
            _drops.Clear();
            SoldTotal = 0f;
        }

        public Skin GetSkin(int index)
        {
            return _drops[index].Skin;
        }

        public bool IsSold(int index)
        {
            return _drops[index].IsSold;
        }

        public void MarkSold(int index, float amount)
        {
            _drops[index].IsSold = true;
            SoldTotal = MoneyMath.RoundToMoney(SoldTotal + amount);
        }

        public List<int> GetUnsoldIndices()
        {
            var indices = new List<int>();

            for (var index = 0; index < _drops.Count; index++)
            {
                if (_drops[index].IsSold == false)
                {
                    indices.Add(index);
                }
            }

            return indices;
        }

        public List<Skin> GetUnsoldSkins()
        {
            var skins = new List<Skin>();

            foreach (var drop in _drops)
            {
                if (drop.IsSold == false)
                {
                    skins.Add(drop.Skin);
                }
            }

            return skins;
        }

        private class Drop
        {
            public readonly Skin Skin;
            public bool IsSold;

            public Drop(Skin skin)
            {
                Skin = skin;
            }
        }
    }
}

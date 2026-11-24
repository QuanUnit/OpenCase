using System.Collections.Generic;
using Features.Cases.Data;
using Features.Skins.Data;
using NUnit.Framework;
using Shared.Identifiers;

namespace Features.Cases.Tests.EditMode
{
    public class CaseDropsResultTests
    {
        private const float Tolerance = 0.0001f;

        private List<Skin> CreateSkins(int count)
        {
            var skins = new List<Skin>();

            for (var index = 0; index < count; index++)
            {
                skins.Add(new Skin(EntityIds.CreateUnique<SkinId>("TestSkin"), EntityIds.CreateUnique<SkinInstanceId>(SkinInstanceId.EntityName), 0.1f, index + 1f, false, false));
            }

            return skins;
        }

        [Test]
        public void Start_Skins_AllUnsoldWithZeroSoldTotal()
        {
            var result = new CaseDropsResult();
            var skins = CreateSkins(3);

            result.Start(skins);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(new List<int>() { 0, 1, 2 }, result.GetUnsoldIndices());
            Assert.AreEqual(skins, result.GetUnsoldSkins());
            Assert.AreEqual(0f, result.SoldTotal, Tolerance);
        }

        [Test]
        public void MarkSold_OneDrop_ExcludedFromUnsoldAndAddedToSoldTotal()
        {
            var result = new CaseDropsResult();
            var skins = CreateSkins(3);
            result.Start(skins);

            result.MarkSold(1, 2.5f);

            Assert.IsTrue(result.IsSold(1) == true);
            Assert.IsTrue(result.IsSold(0) == false);
            Assert.AreEqual(new List<int>() { 0, 2 }, result.GetUnsoldIndices());
            Assert.AreEqual(new List<Skin>() { skins[0], skins[2] }, result.GetUnsoldSkins());
            Assert.AreEqual(2.5f, result.SoldTotal, Tolerance);
        }

        [Test]
        public void MarkSold_PartialThenRest_SoldTotalCoversEverySale()
        {
            var result = new CaseDropsResult();
            result.Start(CreateSkins(5));

            result.MarkSold(1, 2.73f);

            foreach (var index in result.GetUnsoldIndices())
            {
                result.MarkSold(index, 0.01f + index * 0.1f);
            }

            Assert.AreEqual(0, result.GetUnsoldIndices().Count);
            Assert.AreEqual(3.67f, result.SoldTotal, Tolerance);
        }

        [Test]
        public void Start_AfterSales_ResetsState()
        {
            var result = new CaseDropsResult();
            result.Start(CreateSkins(2));
            result.MarkSold(0, 1f);

            var skins = CreateSkins(4);
            result.Start(skins);

            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(4, result.GetUnsoldIndices().Count);
            Assert.AreEqual(0f, result.SoldTotal, Tolerance);
            Assert.AreSame(skins[3], result.GetSkin(3));
        }

        [Test]
        public void Clear_AfterStart_Empty()
        {
            var result = new CaseDropsResult();
            result.Start(CreateSkins(2));
            result.MarkSold(0, 1f);

            result.Clear();

            Assert.AreEqual(0, result.Count);
            Assert.AreEqual(0, result.GetUnsoldSkins().Count);
            Assert.AreEqual(0f, result.SoldTotal, Tolerance);
        }
    }
}

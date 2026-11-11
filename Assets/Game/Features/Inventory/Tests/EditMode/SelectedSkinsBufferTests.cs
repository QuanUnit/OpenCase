using System.Linq;
using Features.Inventory.Data;
using Features.Skins.Data;
using NUnit.Framework;
using QCore.Tools.Identifiers;

namespace Features.Inventory.Tests.EditMode
{
    public class SelectedSkinsBufferTests
    {
        private Skin CreateSkin(float price)
        {
            return new Skin(QGuid.CreateNew(), QGuid.CreateNew(), 0.1f, price, false, false);
        }

        [Test]
        public void AddSelected_TwoSkins_SumsPrices()
        {
            var buffer = new SelectedSkinsBuffer();

            buffer.AddSelected(CreateSkin(5f));
            buffer.AddSelected(CreateSkin(7.5f));

            Assert.AreEqual(12.5f, buffer.TotalPrice);
        }

        [Test]
        public void AddSelected_SameSkinTwice_CountsOnce()
        {
            var buffer = new SelectedSkinsBuffer();
            var skin = CreateSkin(5f);

            buffer.AddSelected(skin);
            buffer.AddSelected(skin);

            Assert.AreEqual(5f, buffer.TotalPrice);
            Assert.AreEqual(1, buffer.Skins.Count());
        }

        [Test]
        public void RemoveSelected_NotSelected_KeepsTotal()
        {
            var buffer = new SelectedSkinsBuffer();
            buffer.AddSelected(CreateSkin(5f));

            buffer.RemoveSelected(CreateSkin(3f));

            Assert.AreEqual(5f, buffer.TotalPrice);
        }

        [Test]
        public void Clear_ResetsTotalAndSkins()
        {
            var buffer = new SelectedSkinsBuffer();
            buffer.AddSelected(CreateSkin(5f));

            buffer.Clear();

            Assert.AreEqual(0f, buffer.TotalPrice);
            Assert.IsTrue(buffer.Skins.Any() == false);
        }
    }
}

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
        public void AddSelected_TwoSkins_StoresBoth()
        {
            var buffer = new SelectedSkinsBuffer();
            var first = CreateSkin(5f);
            var second = CreateSkin(7.5f);

            buffer.AddSelected(first);
            buffer.AddSelected(second);

            Assert.AreEqual(2, buffer.Skins.Count());
            Assert.IsTrue(buffer.Skins.Contains(first) == true);
            Assert.IsTrue(buffer.Skins.Contains(second) == true);
        }

        [Test]
        public void AddSelected_SameSkinTwice_CountsOnce()
        {
            var buffer = new SelectedSkinsBuffer();
            var skin = CreateSkin(5f);

            buffer.AddSelected(skin);
            buffer.AddSelected(skin);

            Assert.AreEqual(1, buffer.Skins.Count());
        }

        [Test]
        public void RemoveSelected_NotSelected_KeepsSelection()
        {
            var buffer = new SelectedSkinsBuffer();
            var skin = CreateSkin(5f);
            buffer.AddSelected(skin);

            buffer.RemoveSelected(CreateSkin(3f));

            Assert.AreEqual(1, buffer.Skins.Count());
            Assert.IsTrue(buffer.Skins.Contains(skin) == true);
        }

        [Test]
        public void RemoveSelected_Selected_RemovesSkin()
        {
            var buffer = new SelectedSkinsBuffer();
            var skin = CreateSkin(5f);
            buffer.AddSelected(skin);

            buffer.RemoveSelected(skin);

            Assert.IsTrue(buffer.Skins.Any() == false);
        }

        [Test]
        public void Clear_ResetsSkins()
        {
            var buffer = new SelectedSkinsBuffer();
            buffer.AddSelected(CreateSkin(5f));

            buffer.Clear();

            Assert.IsTrue(buffer.Skins.Any() == false);
        }
    }
}
